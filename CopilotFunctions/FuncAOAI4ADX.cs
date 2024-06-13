using Azure;
using Azure.AI.OpenAI;
using Kusto.Data;
using Kusto.Data.Net.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;

namespace Company.Function
{
    public class FuncAOAI4ADX
    {
        private readonly ILogger<FuncAOAI4ADX> _logger;

        public FuncAOAI4ADX(ILogger<FuncAOAI4ADX> logger)
        {
            _logger = logger;
        }

        [Function("FuncAOAI4ADX")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("in FuncAOAI4ADX v3");

            var query = !string.IsNullOrEmpty(req.QueryString.Value) ? System.Web.HttpUtility.ParseQueryString(req.QueryString.Value) : null;
            if (query == null)
            {
                return new BadRequestObjectResult("Check Request queries, they should contain 'kqlQuery'");
            }

            var usermessage = query.Get("usermessage");
            var prompt = query.Get("prompt");

            if (!string.IsNullOrEmpty(usermessage) && !string.IsNullOrEmpty(prompt))
            {
                _logger.LogInformation($"usermessage : {usermessage}");

                var clientId = Environment.GetEnvironmentVariable("Fabric_ClientId", EnvironmentVariableTarget.Process);
                var clientSecret = Environment.GetEnvironmentVariable("Fabric_ClientSecret", EnvironmentVariableTarget.Process);
                var tenantId = Environment.GetEnvironmentVariable("Fabric_TenantId", EnvironmentVariableTarget.Process);
                var clusterUri = Environment.GetEnvironmentVariable("Fabric_ClusterUri", EnvironmentVariableTarget.Process);
                var databaseName = Environment.GetEnvironmentVariable("Fabric_DatabaseName", EnvironmentVariableTarget.Process);
                var tableName = Environment.GetEnvironmentVariable("Fabric_TableName", EnvironmentVariableTarget.Process);

                var openaiUrl = Environment.GetEnvironmentVariable("Openai_Url", EnvironmentVariableTarget.Process);
                var openaiApiKey = Environment.GetEnvironmentVariable("Openai_ApiKey", EnvironmentVariableTarget.Process);
                var openaiDeploymentName = Environment.GetEnvironmentVariable("Openai_DeploymentName", EnvironmentVariableTarget.Process);

                if (openaiUrl != null && openaiUrl.Length > 0 && openaiApiKey != null && openaiApiKey.Length > 0)
                {
                    var kustoConnectionStringBuilder = new KustoConnectionStringBuilder(clusterUri)
                        .WithAadApplicationKeyAuthentication(clientId, clientSecret, tenantId);

                    _logger.LogInformation("kustoConnectionStringBuilder initiated");
                    string? kqlResult = "";

                    try
                    {
                        // We start by getting the schema of the table from ADX or Fabric KQL Database
                        string kustoCommand = ".show table " + tableName + " schema as json";
                        string jsonSchemaTable = "";

                        using (var kustoClient = KustoClientFactory.CreateCslQueryProvider(kustoConnectionStringBuilder))
                        {
                            using (var kustoResponse = kustoClient.ExecuteQuery(databaseName, kustoCommand, null))
                            {
                                while (kustoResponse.Read())
                                {
                                    jsonSchemaTable = kustoResponse.GetString(1);
                                }
                            }
                        }

                        _logger.LogInformation($"kustoCommand executed. We received this schema {jsonSchemaTable}");

                        // If we received a valid schema we call Azure OpenAI to translate the user message into a KQL query
                        if (jsonSchemaTable != string.Empty)
                        {
                            OpenAIClient client = new OpenAIClient(new Uri(openaiUrl), new AzureKeyCredential(openaiApiKey));
                            _logger.LogInformation($"Connected to Azure OpenAI");

                            string formattedSystemMessage = string.Format(prompt, tableName, jsonSchemaTable);

                            Response<ChatCompletions> responseWithoutStream = await client.GetChatCompletionsAsync(
                                new ChatCompletionsOptions()
                                {
                                    DeploymentName = openaiDeploymentName,
                                    Messages =
                                    {
                              new ChatRequestSystemMessage(formattedSystemMessage),
                              new ChatRequestUserMessage(usermessage)
                                    },
                                    Temperature = (float)0.7,
                                    MaxTokens = 800,
                                    NucleusSamplingFactor = (float)0.95,
                                    FrequencyPenalty = 0,
                                    PresencePenalty = 0,
                                });

                            _logger.LogInformation($"Response received from Azure OpenAI");
                            ChatCompletions response = responseWithoutStream.Value;

                            if (response == null || response.Choices == null || response.Choices.Count == 0)
                            {
                                _logger.LogInformation($"The Response from Azure OpenAI is empty");
                            }
                            else
                            {
                                var chatChoice = response.Choices.First();

                                if (chatChoice != null)
                                {
                                    var curratedResponse = chatChoice.Message.Content.Replace("```json", "").Replace("```", "").Trim();
                                    var aoaiContent = JsonConvert.DeserializeObject<AOAIResponse>(curratedResponse);
                                    
                                    _logger.LogInformation($"Result received with Query: {aoaiContent?.query}, Error: {aoaiContent?.error}");
                                    kqlResult = aoaiContent?.query ?? aoaiContent?.error;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error : {ex.Message}");
                        return new BadRequestObjectResult($"Error : {ex.Message}");
                    }
                    _logger.LogInformation($"ADX Query from Azure OpenAI: {kqlResult}");
                    return new OkObjectResult(kqlResult);
                }
                else
                {
                    return new BadRequestObjectResult("Environment Properties are not properly set");
                }
            }
            else
            {
                return new BadRequestObjectResult("Check Request queries, they should contain 'usermessage' and 'prompt'");
            }
        }
    }
}
