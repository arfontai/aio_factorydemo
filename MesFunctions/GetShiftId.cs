using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Dynamic;
using Newtonsoft.Json;
using System.Globalization;

namespace AIOFactoryDemo
{
    public class Payload
    {
        public DateTime timestamp { get; set; }
        public string? site { get; set; }
        public string? line { get; set; }
    }
    
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

            // string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            // Payload? payload = string.IsNullOrEmpty(requestBody) ? null : JsonConvert.DeserializeObject<Payload>(requestBody);

            req.Headers.TryGetValue("site", out var headers_site);
            req.Headers.TryGetValue("line", out var headers_line);
            req.Headers.TryGetValue("time", out var headers_time);

            string site = !string.IsNullOrEmpty(headers_site) ? headers_site.ToString() : "Chambery"; // default to Seattle if we don't receive a value for site
            string line = !string.IsNullOrEmpty(headers_line) ? headers_line.ToString() : "Line1"; // default to Line1 if we don't receive a value for line
            string timeFormat = "yyyy-MM-ddTHH:mm:ss."; // 2021-09-01T08:00:00.000Z format used with the AIO sample
            string time;
            try
            {
                time = DateTime.ParseExact(headers_time.ToString(), timeFormat, CultureInfo.InvariantCulture).ToString("HH:mm:ss");
            }
            catch
            {
                time = DateTime.Now.ToString("HH:mm:ss");
            }

            // string line = payload?.site ?? "Seattle"; // default to Seattle if we don't receive a value for site
            // string line = payload?.line ?? "Line1"; // default to Line1 if we don't receive a value for line
            // string time = payload?.timestamp.ToString("HH:mm:ss") ?? DateTime.Now.ToString("HH:mm:ss");
            var shift = ShiftDataListHelper.GetShift(site, line, time);

            dynamic result = new ExpandoObject();
            if (shift != null)
            {
                _logger.LogInformation($"Shift {shift.shiftid} returned");
                result.shiftid = shift.shiftid;
            }
            else
            {
                _logger.LogInformation("No Shift returned");
                result.shiftid = $"N/A for {site} at {time}";
            }

            return new OkObjectResult(result);
        }
    }
}
