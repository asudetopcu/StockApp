using MySql.Data.MySqlClient;
using StockApp.Data;
using StockApp.Models;
using System;
using System.Data;
using System.Windows.Controls;
using System.Collections.Generic;


namespace StockApp.Services
{
    public class LogService
    {
        /// <summary>
        /// Log oluşturmak için kullanılan metod.
        /// </summary>
        public void AddLog(int customerId, int orderId, string logType, string logDetails)
        {
            try
            {
                // LogType doğrulama
                if (logType != "Info" && logType != "Warning" && logType != "Error")
                {
                    throw new ArgumentException("Invalid LogType value");
                }

                // Log ekleme sorgusu
                string insertLogQuery = $@"
                    INSERT INTO Logs (CustomerID, OrderID, LogType, LogDetails)
                    VALUES ({customerId}, {orderId}, '{logType}', '{logDetails}')";

                // Sorguyu çalıştır
                DatabaseHelper.ExecuteNonQuery(insertLogQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while adding log: {ex.Message}");
            }
        }
        public void LoadLogs(DataGrid dataGrid)
        {
            string query = "SELECT * FROM Logs ORDER BY LogDate DESC";

            try
            {
                DataTable logs = DatabaseHelper.ExecuteQuery(query);
                dataGrid.ItemsSource = logs.DefaultView; // DataGrid'e bağlama
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading logs: {ex.Message}");
            }
        }

    }
}

