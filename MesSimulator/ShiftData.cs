using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AIOFactoryDemo
{
    public enum WorkType
    {
        Prod,
        Maintenance,
        Rework
    }
    
    public class ShiftData
    {
        public required string Site { get; set; }
        public int Shift { get; set; }
        public required string Operator { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public WorkType WorkType { get; set; }
        public int PackagedProductTarget { get; set; }
        public int PerformanceTarget { get; set; }
    }

    public class ShiftDataListHelper
    {
        private static List<ShiftData> shifts = new List<ShiftData>
        {
            new ShiftData { Site = "Seattle", Shift = 1, Operator = "Alice",  Start = DateTime.Today,              End = DateTime.Today.AddHours(7).AddMinutes(59),  WorkType = WorkType.Prod, PackagedProductTarget = 1000, PerformanceTarget = 95},
            new ShiftData { Site = "Seattle", Shift = 2, Operator = "Bob",    Start = DateTime.Today.AddHours(8),  End = DateTime.Today.AddHours(15).AddMinutes(59), WorkType = WorkType.Prod, PackagedProductTarget = 1000, PerformanceTarget = 95},
            new ShiftData { Site = "Seattle", Shift = 3, Operator = "Charly", Start = DateTime.Today.AddHours(16), End = DateTime.Today.AddHours(23).AddMinutes(59), WorkType = WorkType.Prod, PackagedProductTarget = 1000, PerformanceTarget = 95}
        };    

        public static ShiftData? GetShift(string site, string time)
        {
            if (shifts == null || shifts.Count == 0)
            {
                return null;
            }
            var shift = shifts.FirstOrDefault(s => s.Site == site && s.Start <= DateTime.Parse(time) && s.End >= DateTime.Parse(time));
            return shift;
        }

        public static List<ShiftData>? GetShifts(string site)
        {
            if (shifts == null || shifts.Count == 0)
            {
                return null;
            }
            var shift = shifts.Where(s => s.Site == site).ToList();
            return shift;
        }
    }
}