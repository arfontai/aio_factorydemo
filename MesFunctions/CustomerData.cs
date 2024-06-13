using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AIOFactoryDemo
{    
    public class CustomerData
    {
        public required string shiftid { get; set; }
        public required string customer { get; set; }
        public required string purchaseorder { get; set; }
        public required string productid { get; set; }
        public int quantity { get; set; }
    }

    public class CustomerDataListHelper
    {
        private static List<CustomerData>? Customers = null;

        public static void InitCustomers()
        {
            Customers = new List<CustomerData>
            {
                new CustomerData { shiftid = "Chb1", customer = "NorthPlace",   purchaseorder = "4500001710", productid = "Beaufort", quantity = 3000 },
                new CustomerData { shiftid = "Chb2", customer = "SouthVillage", purchaseorder = "4500006543", productid = "Beaufort", quantity = 2500 },
                new CustomerData { shiftid = "Chb3", customer = "EastStore",    purchaseorder = "4500008876", productid = "Beaufort", quantity = 1000 }
            };
        }

        public static List<CustomerData>? GetCustomers(string shiftId)
        {
            InitCustomers();

            var Customer = Customers?.Where(m => m.shiftid == shiftId).ToList();
            return Customer;
        }

        public static List<CustomerData>? GetCustomers()
        {
            InitCustomers();
            return Customers;
        }
    }
}