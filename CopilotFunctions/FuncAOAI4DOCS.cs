using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Company.Function
{
    public class AISearchResult
    {
        public string? content { get; set; }
        public string? filepath { get; set; }
    }
    
    public class FuncAOAI4DOCS
    {
        private readonly ILogger<FuncAOAI4DOCS> _logger;

        public FuncAOAI4DOCS(ILogger<FuncAOAI4DOCS> logger)
        {
            _logger = logger;
        }

        [Function("FuncAOAI4DOCS")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("in FuncAOAI4DOCS");

            var query = !string.IsNullOrEmpty(req.QueryString.Value) ? System.Web.HttpUtility.ParseQueryString(req.QueryString.Value) : null;
            if (query == null)
            {
                return new BadRequestObjectResult("Check Request queries, they should contain 'userprompt'");
            }

            var openaiUrl = Environment.GetEnvironmentVariable("Openai_Url", EnvironmentVariableTarget.Process);
            var openaiApiKey = Environment.GetEnvironmentVariable("Openai_ApiKey", EnvironmentVariableTarget.Process);
            var openaiDeploymentName = Environment.GetEnvironmentVariable("Openai_DeploymentName", EnvironmentVariableTarget.Process);

            var searchEndpoint = Environment.GetEnvironmentVariable("AISearch_Endpoint", EnvironmentVariableTarget.Process);
            var searchKey = Environment.GetEnvironmentVariable("AISearch_Key", EnvironmentVariableTarget.Process);
            var searchIndexName = Environment.GetEnvironmentVariable("AISearch_IndexName", EnvironmentVariableTarget.Process);

            var userprompt = query.Get("userprompt");


            if (!string.IsNullOrEmpty(userprompt))
            {
                try
                {
                    OpenAIClient client = new OpenAIClient(new Uri(openaiUrl), new AzureKeyCredential(openaiApiKey!));
                    
                    var fieldMappingOptions = new AzureSearchIndexFieldMappingOptions()
                    {
                        TitleFieldName = "metadata_title",
                        UrlFieldName = "metadata_storage_name",
                        FilepathFieldName = "metadata_storage_path"
                    };
                    fieldMappingOptions.ContentFieldNames.Add("metadata_storage_size");
                    fieldMappingOptions.ContentFieldNames.Add("metadata_storage_last_modified");
                    fieldMappingOptions.ContentFieldNames.Add("metadata_storage_last_modified");
                    fieldMappingOptions.ContentFieldNames.Add("metadata_language");

                    var chatCompletionsOptions = new ChatCompletionsOptions()
                    {
                        Messages =
                        {
                            new ChatRequestUserMessage(userprompt)
                        },

                        AzureExtensionsOptions = new AzureChatExtensionsOptions()
                        {
                            Extensions =
                            {
                                new AzureSearchChatExtensionConfiguration()
                                {
                                    Authentication = new OnYourDataApiKeyAuthenticationOptions(searchKey),
                                    SearchEndpoint = new Uri(searchEndpoint),
                                    // RoleInformation = userprompt,
                                    IndexName = searchIndexName,
                                    // QueryType =  AzureSearchQueryType.Simple
                                    FieldMappingOptions = fieldMappingOptions
                                }
                            },
                        },
                        DeploymentName = openaiDeploymentName,
                        MaxTokens = 800,
                        Temperature = (float)0.7,
                    };

                    var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
                    var chatChoice = response.Value.Choices.First();
                    var content = chatChoice.Message.Content;

                    _logger.LogInformation($"Content: {chatChoice?.Message?.Content}");
                    string filepath = "";
                    foreach (var citation in chatChoice?.Message?.AzureExtensionsContext?.Citations)
                    {
                        string encodedUrl = citation.Filepath.TrimEnd('0');
                        var bytes = Convert.FromBase64String(encodedUrl);
                        var decodedUrl = Encoding.UTF8.GetString(bytes);
                        filepath = string.IsNullOrEmpty(filepath) ? decodedUrl : filepath;
                        
                        _logger.LogInformation($"[citation] Title: {citation.Title}, Url: {filepath}");
                        _logger.LogInformation($"Content: {citation.Content}");
                    }

                    AISearchResult result = new AISearchResult();
                    result.content = chatChoice?.Message?.Content;
                    result.filepath = filepath;

                    return new OkObjectResult(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in FuncAOAI4DOCS: {ex.Message}");
                    return new BadRequestObjectResult("Error in FuncAOAI4DOCS");
                }
            }
            else
            {
                return new BadRequestObjectResult("Environment Properties are not properly set");
            }
        }
    }
}
