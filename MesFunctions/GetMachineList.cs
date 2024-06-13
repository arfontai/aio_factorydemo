using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AIOFactoryDemo
{
    public class GetMachineList
    {
        private readonly ILogger<GetMachineList> _logger;

        public GetMachineList(ILogger<GetMachineList> logger)
        {
            _logger = logger;
        }

        [Function("GetMachineList")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "MES/GetMachineList")] HttpRequest req)
        {
            _logger.LogInformation("In MES/GetMachineList function");

            var Machines = MachineDataListHelper.GetMachines();
            if (Machines != null)
            {
                _logger.LogInformation($"{Machines.Count} Machines returned");
            }
            else
            {
                _logger.LogInformation("No Machines returned");
            }

            return new OkObjectResult(Machines);
        }
    }
}
