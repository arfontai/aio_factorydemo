using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AIOFactoryDemo
{
    public class GetShiftList
    {
        private readonly ILogger<GetShiftList> _logger;

        public GetShiftList(ILogger<GetShiftList> logger)
        {
            _logger = logger;
        }

        [Function("GetShiftList")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "MES/GetShiftList")] HttpRequest req)
        {
            _logger.LogInformation("In MES/GetShiftList function");

            // string site = req.Query["site"];
            string site = "Seattle";

            var shifts = ShiftDataListHelper.GetShifts(site);
            if (shifts != null)
            {
                _logger.LogInformation($"{shifts.Count} Shifts returned");
            }
            else
            {
                _logger.LogInformation("No Shifts returned");
            }

            return new OkObjectResult(shifts);
        }
    }
}
