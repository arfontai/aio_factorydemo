using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AIOFactoryDemo
{
    public class GetShiftId
    {
        private readonly ILogger<GetShiftId> _logger;

        public GetShiftId(ILogger<GetShiftId> logger)
        {
            _logger = logger;
        }

        [Function("GetShiftId")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "MES/GetShiftId")] HttpRequest req)
        {
            _logger.LogInformation("In MES/GetShiftId function");

            // string site = req.Query["site"];
            string site = "Seattle";
                        
            // string time = req.Query["time"];
            string time = DateTime.Now.ToString("HH:mm:ss");
            
            var shift = ShiftDataListHelper.GetShift(site, time);
            if (shift != null)
            {
                 _logger.LogInformation($"Shift {shift.Shift} with Operator: {shift.Operator} returned");
                 return new OkObjectResult(shift.Shift);
            }           
            else
            {
                _logger.LogInformation("No Shift returned");
                return new NotFoundResult();
            }
        }
    }
}
