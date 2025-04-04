﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockApp.Models
{
    public enum LogType
    {
        Completed,
        Approved,
        Error,
        Info
    }
    public class Log
    {
        public int LogID { get; set; }
        public int CustomerID { get; set; } // Foreign Key
        public int? OrderID { get; set; } // Foreign Key (nullable, as not all logs are related to orders)
        public DateTime LogDate { get; set; }
        public LogType LogType { get; set; } // Info, Warning, or Error
        public string LogDetails { get; set; }

        // Optional navigation properties
        public Customer? Customer { get; set; } // Nullable yapıldı
        public Order? Order { get; set; } // Nullable yapıldı
    }

}
