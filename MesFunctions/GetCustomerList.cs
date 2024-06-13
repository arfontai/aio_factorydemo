using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AIOFactoryDemo
{
    public class GetCustomerList
    {
        private readonly ILogger<GetCustomerList> _logger;

        public GetCustomerList(ILogger<GetCustomerList> logger)
        {
            _logger = logger;
        }

        [Function("GetCustomerList")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "MES/GetCustomerList")] HttpRequest req)
        {
            _logger.LogInformation("In MES/GetCustomerList function");

            var Customers = CustomerDataListHelper.GetCustomers();
            if (Customers != null)
            {
                _logger.LogInformation($"{Customers.Count} Customers returned");
            }
            else
            {
                _logger.LogInformation("No Customers returned");
            }

            return new OkObjectResult(Customers);
        }
    }
}
