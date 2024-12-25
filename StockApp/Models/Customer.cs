using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockApp.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } // Unique
        public string Password { get; set; }
        public decimal Budget { get; set; }
        public string CustomerType { get; set; } // Premium or Standard
        public int TotalSpent { get; set; }
        public bool IsAdmin { get; set; } // TRUE if admin, FALSE otherwise
    }

}
