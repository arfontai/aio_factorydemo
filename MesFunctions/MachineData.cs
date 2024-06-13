using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AIOFactoryDemo
{    
    public class MachineData
    {
        public required string assetid { get; set; }
        public required string site { get; set; }
        public required string line { get; set; }
        public required string workbench { get; set; }
        public required string manufacturer { get; set; }
        public DateTime lastmaintenancedate { get; set; }
        public int timetonextmaintenanceindays { get; set; }
        public int idealcycletimeinms { get; set; }
    }

    public class MachineDataListHelper
    {
        private static List<MachineData>? Machines = null;

        public static void InitMachines()
        {
            Machines = new List<MachineData>
            {
                new MachineData { assetid = "Chambery_Line1_Assembly", site = "Chambery", line = "Line1", workbench = "Assembly", manufacturer = "Fanuc", lastmaintenancedate = DateTime.Today.AddDays(-30), timetonextmaintenanceindays = 12, idealcycletimeinms = 4000 }
            };
        }

        public static MachineData? GetMachine(string assetId)
        {
            InitMachines();

            var machine = Machines?.FirstOrDefault(m => m.assetid == assetId);
            return machine;
        }

        public static List<MachineData>? GetMachines()
        {
            InitMachines();

            return Machines;
        }
    }
}