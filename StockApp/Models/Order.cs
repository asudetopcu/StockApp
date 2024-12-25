using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockApp.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; } // Foreign Key
        public int ProductID { get; set; } // Foreign Key
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } // Pending, Completed, or Cancelled

        // Optional navigation properties (if using Entity Framework or similar ORM)
        public Customer Customer { get; set; }
        public Product Product { get; set; }
    }

}
