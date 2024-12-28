using MySql.Data.MySqlClient;
using StockApp.Data;
using StockApp.Models;
using System;

namespace StockApp.Services
{
    public class Logger
    {
        /// <summary>
        /// Log oluşturmak için kullanılan metod.
        /// </summary>
        public static void Log(int? customerId, int? orderId, LogType logType, string details)
        {
            string logQuery = "INSERT INTO Logs (CustomerID, OrderID, LogDate, LogType, LogDetails) " +
                              "VALUES (@CustomerID, @OrderID, NOW(), @LogType, @LogDetails)";
            var parameters = new[]
            {
                new MySqlParameter("@CustomerID", customerId ?? (object)DBNull.Value),
                new MySqlParameter("@OrderID", orderId ?? (object)DBNull.Value),
                new MySqlParameter("@LogType", logType.ToString()), // Enum'ı string olarak kaydet
                new MySqlParameter("@LogDetails", details)
            };

            try
            {
                DatabaseHelper.ExecuteNonQueryAsync(logQuery, parameters);
            }
            catch (Exception ex)
            {
                // Eğer loglama sırasında bir hata oluşursa, hata mesajını konsola yazdır
                Console.WriteLine($"Log Error: {ex.Message}");
                Console.WriteLine($"Details: {details}");
            }
        }

        /// <summary>
        /// Başarıyla tamamlanmış bir işlem için log kaydı oluşturur.
        /// </summary>
        public static void LogSuccess(int? customerId, int? orderId, string details)
        {
            Log(customerId, orderId, LogType.Info, details);
        }

        /// <summary>
        /// Hata durumlarında log kaydı oluşturur.
        /// </summary>
        public static void LogError(int? customerId, int? orderId, string details)
        {
            Log(customerId, orderId, LogType.Error, details);
        }

        /// <summary>
        /// Uyarı durumları için log kaydı oluşturur.
        /// </summary>
        public static void LogWarning(int? customerId, int? orderId, string details)
        {
            Log(customerId, orderId, LogType.Error, details);
        }
    }
}

