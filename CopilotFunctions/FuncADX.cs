using Func;
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
    public class FuncADX
    {
        private readonly ILogger<FuncADX> _logger;

        public FuncADX(ILogger<FuncADX> logger)
        {
            _logger = logger;
        }

        [Function("FuncADX")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("in FuncADX v2");

            var query = !string.IsNullOrEmpty(req.QueryString.Value) ? System.Web.HttpUtility.ParseQueryString(req.QueryString.Value) : null;
            if (query == null)
            {
                return new BadRequestObjectResult("Check Request queries, they should contain 'kqlQuery'");
            }

            var kqlQuery = query.Get("kqlquery");
            if (!string.IsNullOrEmpty(kqlQuery))
            {
                var clientId = Environment.GetEnvironmentVariable("Fabric_ClientId", EnvironmentVariableTarget.Process);
                var clientSecret = Environment.GetEnvironmentVariable("Fabric_ClientSecret", EnvironmentVariableTarget.Process);
                var tenantId = Environment.GetEnvironmentVariable("Fabric_TenantId", EnvironmentVariableTarget.Process);
                var clusterUri = Environment.GetEnvironmentVariable("Fabric_ClusterUri", EnvironmentVariableTarget.Process);
                var databaseName = Environment.GetEnvironmentVariable("Fabric_DatabaseName", EnvironmentVariableTarget.Process);

                if (clientSecret != null && clientSecret.Length > 0)
                {
                    var kustoConnectionStringBuilder = new KustoConnectionStringBuilder(clusterUri)
                        .WithAadApplicationKeyAuthentication(clientId, clientSecret, tenantId);
                    string jsonResult = "";

                    try
                    {
                        using (var kustoClient = KustoClientFactory.CreateCslQueryProvider(kustoConnectionStringBuilder))
                        {
                            using (var kustoResponse = kustoClient.ExecuteQuery(databaseName, kqlQuery, null))
                            {
                                var table = new DataTable();
                                table.Load(kustoResponse);

                                jsonResult = JsonConvert.SerializeObject(ADXUtils.ConvertDecimal(table), Formatting.Indented);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return new BadRequestObjectResult($"Error : {ex.Message}");
                    }
                    _logger.LogInformation($"Result from ADX : {jsonResult}");
                    return new OkObjectResult(jsonResult);
                }
                else
                {
                    return new BadRequestObjectResult("Environment Properties are not properly set");
                }
            }
            else
            {
                return new BadRequestObjectResult("Check Request queries, they should contain 'kqlquery'");
            }
        }
    }
}
