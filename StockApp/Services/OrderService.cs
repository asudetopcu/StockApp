using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using StockApp.Data;
using StockApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockApp.Services
{
    public class OrderService
    {
        private readonly PriorityQueue<Customer, double> customerQueue = new PriorityQueue<Customer, double>();

        public async Task PurchaseProductAsync(int customerId, int productId, int quantity)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // Stok kontrolü
                        string checkStockQuery = "SELECT Stock FROM Products WHERE ProductID = @ProductID FOR UPDATE";
                        var checkStockCommand = new MySqlCommand(checkStockQuery, connection, transaction);
                        checkStockCommand.Parameters.AddWithValue("@ProductID", productId);
                        int stock = Convert.ToInt32(await checkStockCommand.ExecuteScalarAsync());

                        if (stock < quantity)
                        {
                            throw new Exception("Insufficient stock!");
                        }

                        // Ürün fiyatını al
                        string getPriceQuery = "SELECT Price FROM Products WHERE ProductID = @ProductID";
                        var getPriceCommand = new MySqlCommand(getPriceQuery, connection, transaction);
                        getPriceCommand.Parameters.AddWithValue("@ProductID", productId);
                        decimal price = Convert.ToDecimal(await getPriceCommand.ExecuteScalarAsync());

                        decimal totalCost = price * quantity;

                        // Müşteri bakiyesi kontrolü
                        string checkBudgetQuery = "SELECT Budget FROM Customers WHERE CustomerID = @CustomerID FOR UPDATE";
                        var checkBudgetCommand = new MySqlCommand(checkBudgetQuery, connection, transaction);
                        checkBudgetCommand.Parameters.AddWithValue("@CustomerID", customerId);
                        decimal budget = Convert.ToDecimal(await checkBudgetCommand.ExecuteScalarAsync());

                        if (budget < totalCost)
                        {
                            throw new Exception("Insufficient budget!");
                        }

                        // Stok güncelleme
                        string updateStockQuery = "UPDATE Products SET Stock = Stock - @Quantity WHERE ProductID = @ProductID";
                        var updateStockCommand = new MySqlCommand(updateStockQuery, connection, transaction);
                        updateStockCommand.Parameters.AddWithValue("@Quantity", quantity);
                        updateStockCommand.Parameters.AddWithValue("@ProductID", productId);
                        await updateStockCommand.ExecuteNonQueryAsync();

                        // Sipariş kaydı
                        string insertOrderQuery = "INSERT INTO Orders (CustomerID, ProductID, Quantity, TotalPrice, OrderDate, OrderStatus) " +
                                                  "VALUES (@CustomerID, @ProductID, @Quantity, @TotalPrice, NOW(), @OrderStatus)";
                        var insertOrderCommand = new MySqlCommand(insertOrderQuery, connection, transaction);
                        insertOrderCommand.Parameters.AddWithValue("@CustomerID", customerId);
                        insertOrderCommand.Parameters.AddWithValue("@ProductID", productId);
                        insertOrderCommand.Parameters.AddWithValue("@Quantity", quantity);
                        insertOrderCommand.Parameters.AddWithValue("@TotalPrice", totalCost.ToString(CultureInfo.InvariantCulture));
                        insertOrderCommand.Parameters.AddWithValue("@OrderStatus", "Completed");
                        await insertOrderCommand.ExecuteNonQueryAsync();

                        // Müşteri bakiyesi güncelleme
                        string updateBudgetQuery = "UPDATE Customers SET Budget = Budget - @TotalCost, TotalSpent = TotalSpent + @TotalCost WHERE CustomerID = @CustomerID";
                        var updateBudgetCommand = new MySqlCommand(updateBudgetQuery, connection, transaction);
                        updateBudgetCommand.Parameters.AddWithValue("@TotalCost", totalCost.ToString(CultureInfo.InvariantCulture));
                        updateBudgetCommand.Parameters.AddWithValue("@CustomerID", customerId);
                        await updateBudgetCommand.ExecuteNonQueryAsync();

                        // Sipariş ID'sini al
                        int orderId = Convert.ToInt32(insertOrderCommand.LastInsertedId);

                        // Sipariş işlemi loglama
                        LogOrderProcessing(customerId, orderId, LogType.Completed, "Order completed successfully.");

                        // Transaction'u onayla
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        // Transaction'u geri al
                        await transaction.RollbackAsync();
                        // Hata loglama
                        Logger.LogError(customerId, null, ex.Message);
                        throw new Exception($"Purchase failed: {ex.Message}");
                    }
                }
            }
        }


        // Müşteriyi sıraya ekler
        public void AddCustomerToQueue(Customer customer)
        {
            customer.OrderTime = DateTime.Now; // Sipariş zamanını kaydedin
            int waitTimeInSeconds = 0; // Yeni gelen müşteri için başlangıç bekleme süresi
            double priorityScore = CalculatePriorityScore(customer, waitTimeInSeconds);
            customerQueue.Enqueue(customer, -priorityScore); // Negatif skor, en yüksek önceliği üst sıraya getirir
        }


        // Sıradaki en yüksek öncelikli müşteriyi alır
        public Customer GetNextCustomer()
        {
            return customerQueue.Count > 0 ? customerQueue.Dequeue() : null;
        }

        public double CalculatePriorityScore(Customer customer, int waitTimeInSeconds)
        {
            double baseScore = customer.CustomerType == "Premium" ? 15 : 10; // Temel öncelik skoru
            double waitTimeWeight = 0.5; // Bekleme süresi ağırlığı
            return baseScore + (waitTimeInSeconds * waitTimeWeight);
        }

        // Müşteri önceliklerini günceller
        public void UpdateCustomerPriorities()
        {
            List<(Customer customer, double priorityScore)> tempList = new List<(Customer, double)>();

            while (customerQueue.Count > 0)
            {
                Customer customer = customerQueue.Dequeue();
                int waitTimeInSeconds = (int)(DateTime.Now - customer.OrderTime).TotalSeconds; // Bekleme süresi
                double newPriorityScore = CalculatePriorityScore(customer, waitTimeInSeconds);
                tempList.Add((customer, newPriorityScore));
            }

            foreach (var entry in tempList)
            {
                customerQueue.Enqueue(entry.customer, -entry.priorityScore);
            }
        }

        public void LogOrderProcessing(int customerId, int orderId, LogType logType, string status)
        {
            string logQuery = "INSERT INTO Logs (CustomerID, OrderID, LogDate, LogType, LogDetails) " +
                             "VALUES (@CustomerID, @OrderID, NOW(), @LogTypeStr, @LogDetails)";

            var parameters = new[]
            {
            new MySqlParameter("@CustomerID", customerId),
            new MySqlParameter("@OrderID", orderId),
            new MySqlParameter("@LogTypeStr", logType.ToString()),  // Enum'ı string olarak kaydet
            new MySqlParameter("@LogDetails", $"Order status updated to {status}.")
        };

            try
            {
                DatabaseHelper.ExecuteNonQuery(logQuery, parameters);
            }
            catch (Exception ex)
            {
                // Hata detaylarını logla
                Console.WriteLine($"Query: {logQuery}");
                foreach (var param in parameters)
                {
                    Console.WriteLine($"{param.ParameterName} = {param.Value}");
                }
                throw new Exception($"Log insertion failed: {ex.Message}", ex);
            }
        }

    }
}
