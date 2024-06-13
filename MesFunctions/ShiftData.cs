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
        public required string shiftid { get; set; }
        public required string site { get; set; }
        public required string line { get; set; }
        public int shift { get; set; }
        public required string @operator { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public WorkType worktype { get; set; }
        public int packagedproducttarget { get; set; }
        public int performancetarget { get; set; }
    }

    public class ShiftDataListHelper
    {
        private static List<ShiftData>? Shifts = null;
        
        public static void InitShifts()
        {
            Shifts = new List<ShiftData>
            {
                new ShiftData { shiftid = "Chb1", site = "Chambery", line = "Line1", shift = 1, @operator = "Alice",     start = DateTime.Today,              end = DateTime.Today.AddHours(8),  worktype = WorkType.Prod, packagedproducttarget = 6480, performancetarget = 90},
                new ShiftData { shiftid = "Chb2", site = "Chambery", line = "Line1", shift = 2, @operator = "Bob",       start = DateTime.Today.AddHours(8),  end = DateTime.Today.AddHours(16), worktype = WorkType.Prod, packagedproducttarget = 6480, performancetarget = 90},
                new ShiftData { shiftid = "Chb3", site = "Chambery", line = "Line1", shift = 3, @operator = "Charly",    start = DateTime.Today.AddHours(16), end = DateTime.Today.AddHours(24), worktype = WorkType.Prod, packagedproducttarget = 5760, performancetarget = 80},
                new ShiftData { shiftid = "Anc1", site = "Annecy",   line = "Line1", shift = 1, @operator = "Arthur",    start = DateTime.Today,              end = DateTime.Today.AddHours(8),  worktype = WorkType.Prod, packagedproducttarget = 6480, performancetarget = 90},
                new ShiftData { shiftid = "Anc2", site = "Annecy",   line = "Line1", shift = 2, @operator = "Berenice",  start = DateTime.Today.AddHours(8),  end = DateTime.Today.AddHours(16), worktype = WorkType.Prod, packagedproducttarget = 6480, performancetarget = 90},
                new ShiftData { shiftid = "Anc3", site = "Annecy",   line = "Line1", shift = 3, @operator = "Charlotte", start = DateTime.Today.AddHours(16), end = DateTime.Today.AddHours(24), worktype = WorkType.Prod, packagedproducttarget = 5760, performancetarget = 80}
            };
        }

        public static ShiftData? GetShift(string site, string line, string time)
        {
            InitShifts();

            var shift = Shifts?.FirstOrDefault(s => s.site == site && s.line == line && s.start <= DateTime.Parse(time) && s.end >= DateTime.Parse(time));
            return shift;
        } 

        public static List<ShiftData>? GetShifts(string site)
        {
            InitShifts();
            
            var shift = Shifts?.Where(s => s.site == site).ToList();
            return shift;
        }

        public static List<ShiftData>? GetShifts()
        {
            InitShifts();
            
            return Shifts;
        }
    }
}