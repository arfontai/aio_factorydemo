using Azure;
using Azure.AI.OpenAI;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Company.Function
{
    public class FuncAOAI4FLW
    {
        private readonly ILogger<FuncAOAI4FLW> _logger;

        public FuncAOAI4FLW(ILogger<FuncAOAI4FLW> logger)
        {
            _logger = logger;
        }

        private static bool IsNotNull([NotNullWhen(true)] object? obj) => obj != null;

        [Function("FuncAOAI4FLW")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("in FuncAOAI4FLW");

            var query = !string.IsNullOrEmpty(req.QueryString.Value) ? System.Web.HttpUtility.ParseQueryString(req.QueryString.Value) : null;
            if (query == null)
            {
                return new BadRequestObjectResult("Check Request queries, they should contain 'datasetlocation', 'systemprompt' and 'userprompt'");
            }

            var openaiUrl = Environment.GetEnvironmentVariable("Openai_Url", EnvironmentVariableTarget.Process);
            var openaiApiKey = Environment.GetEnvironmentVariable("Openai_ApiKey", EnvironmentVariableTarget.Process);
            var openaiDeploymentName = Environment.GetEnvironmentVariable("Openai_DeploymentName", EnvironmentVariableTarget.Process);

            var blobEndpoint = Environment.GetEnvironmentVariable("Storage_BlobEndpoint", EnvironmentVariableTarget.Process);
            var accountName = Environment.GetEnvironmentVariable("Storage_AccountName", EnvironmentVariableTarget.Process);
            var accountKey = Environment.GetEnvironmentVariable("Storage_AccountKey", EnvironmentVariableTarget.Process);          
            var containerName = Environment.GetEnvironmentVariable("Storage_ContainerName", EnvironmentVariableTarget.Process);
            
            var datasetlocation = query.Get("datasetlocation");
            var systemprompt = query.Get("systemprompt");
            var userprompt = query.Get("userprompt");

            if (!string.IsNullOrEmpty(blobEndpoint) && !string.IsNullOrEmpty(datasetlocation) && !string.IsNullOrEmpty(systemprompt) && !string.IsNullOrEmpty(userprompt))
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(
                    new Uri(blobEndpoint!),
                    new Azure.Storage.StorageSharedKeyCredential(accountName, accountKey));
                BlobClient blobClient = blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(datasetlocation);

                // Download the blob
                var blobResponse = blobClient.Download();
                string dataset = new System.IO.StreamReader(blobResponse.Value.Content).ReadToEnd();
                _logger.LogInformation($"dataset loaded : {dataset}");
                
                // Delete the blob
                await blobClient.DeleteAsync();

                if (openaiUrl != null && openaiUrl.Length > 0 && openaiApiKey != null && openaiApiKey.Length > 0)
                {
                    OpenAIClient client = new OpenAIClient(new Uri(openaiUrl), new AzureKeyCredential(openaiApiKey!));
                    try
                    {
                        string formattedSystemMessage = systemprompt!;
                        string formattedUserMessage = string.Format(userprompt!, dataset);
                        _logger.LogInformation($"formattedUserMessage : {formattedUserMessage}");

                        Response<ChatCompletions> responseWithoutStream = await client.GetChatCompletionsAsync(
                            new ChatCompletionsOptions()
                            {
                                DeploymentName = openaiDeploymentName,
                                Messages =
                                {
                              new ChatRequestSystemMessage(formattedSystemMessage),
                              new ChatRequestUserMessage(formattedUserMessage)
                                },
                                Temperature = (float)0.7,
                                MaxTokens = 800,
                                NucleusSamplingFactor = (float)0.95,
                                FrequencyPenalty = 0,
                                PresencePenalty = 0
                            });

                        ChatCompletions response = responseWithoutStream.Value;
                        var chatChoice = response.Choices.First();
                        var content = chatChoice.Message.Content;
                        
                        _logger.LogInformation($"content : {content}");
                        return new OkObjectResult(content);
                    }
                    catch (Exception ex)
                    {
                        return new BadRequestObjectResult($"Error : {ex.Message}");
                    }
                }
                else
                {
                    return new BadRequestObjectResult("Environment Properties are not properly set");
                }

            }
            else
            {
                return new BadRequestObjectResult("Check Request queries, they should contain 'datasetlocation', 'systemprompt' and 'userprompt'");
            }
        }
    }
}
