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
using System.Windows.Controls;


namespace StockApp.Services
{
    public class OrderService
    {
        private readonly LogService _logService = new LogService();
        private readonly PriorityQueue<Customer, double> customerQueue = new PriorityQueue<Customer, double>();

        public async Task PurchaseProductAsync(int customerId, int productId, int quantity)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    LogService logService = new LogService();
                    try
                    {
                        // Stok kontrolü
                        string checkStockQuery = "SELECT Stock FROM Products WHERE ProductID = @ProductID FOR UPDATE";
                        var checkStockCommand = new MySqlCommand(checkStockQuery, connection, transaction);
                        checkStockCommand.Parameters.AddWithValue("@ProductID", productId);
                        int stock = Convert.ToInt32(await checkStockCommand.ExecuteScalarAsync());

                        if (stock < quantity)
                        {
                            _logService.AddLog(customerId, 0, "Error", "Ürün stoğu yetersiz");
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

                        // Log ekle (başarılı sipariş)
                        _logService.AddLog(customerId, (int)insertOrderCommand.LastInsertedId, "Info", "Satın alma başarılı");

                        // Transaction'u onayla
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        // Transaction'u geri al
                        await transaction.RollbackAsync();
                        // Hata loglama
                        _logService.AddLog(customerId, 0, "Error", ex.Message);
                        throw new Exception($"Purchase failed: {ex.Message}");
                    }
                }
            }
        }


        // Müşteriyi sıraya ekler
        public void AddCustomerToQueue(Customer customer)
        {
            customer.OrderTime = DateTime.Now; // Sipariş zamanını kaydedin
            int waitTimeInSeconds = 0; // Yeni müşteri için başlangıç bekleme süresi
            double priorityScore = CalculatePriorityScore(customer, waitTimeInSeconds);
            customerQueue.Enqueue(customer, -priorityScore); // Negatif skor, en yüksek önceliği üst sıraya getirir
        }


        // Sıradaki en yüksek öncelikli müşteriyi alır
        public Customer GetNextCustomer()
        {
            if (customerQueue.Count > 0)
            {
                var nextCustomer = customerQueue.Dequeue();
                return nextCustomer;
            }
            return null;
        }


        public double CalculatePriorityScore(Customer customer, int waitTimeInSeconds)
        {
            double baseScore = customer.CustomerType == "Premium" ? 15 : 10; // Premium müşteriler için daha yüksek skor
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

        public async Task UpdateOrderStatusAsync(int customerId, int productId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();

                string query = @"
            UPDATE Orders
            SET OrderStatus = 'Completed'
            WHERE CustomerID = @CustomerID AND ProductID = @ProductID AND OrderStatus = 'Pending'";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    command.Parameters.AddWithValue("@ProductID", productId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateOrdersToCompletedAsync()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();

                string selectQuery = "SELECT OrderID, CustomerID FROM Orders WHERE OrderStatus = 'Pending'";
                var orderList = new List<(int OrderId, int CustomerId)>();

                using (var selectCommand = new MySqlCommand(selectQuery, connection))
                using (var reader = await selectCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int orderId = Convert.ToInt32(reader["OrderID"]);
                        int customerId = Convert.ToInt32(reader["CustomerID"]);
                        orderList.Add((orderId, customerId));
                    }
                }

                string updateQuery = "UPDATE Orders SET OrderStatus = 'Completed' WHERE OrderStatus = 'Pending'";
                using (var updateCommand = new MySqlCommand(updateQuery, connection))
                {
                    await updateCommand.ExecuteNonQueryAsync();
                }

                // Log ekleme
                foreach (var order in orderList)
                {
                    _logService.AddLog(order.CustomerId, order.OrderId, "Info", "Order status updated to Completed.");
                }
            }
        }

        public async Task ProcessNextOrderAsync()
        {
            var nextCustomer = GetNextCustomer();
            if (nextCustomer == null)
            {
                throw new Exception("No customers in the queue.");
            }

            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();

                string query = @"
            SELECT o.OrderID, o.ProductID, o.Quantity
            FROM Orders o
            WHERE o.CustomerID = @CustomerID AND o.OrderStatus = 'Pending'
            ORDER BY o.OrderDate ASC
            LIMIT 1";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", nextCustomer.CustomerID);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int orderId = Convert.ToInt32(reader["OrderID"]);
                            int productId = Convert.ToInt32(reader["ProductID"]);
                            int quantity = Convert.ToInt32(reader["Quantity"]);

                            // Siparişi onayla ve stokları güncelle
                            await ApproveOrderAsync(nextCustomer.CustomerID, orderId, productId, quantity);
                        }
                        else
                        {
                            throw new Exception("No pending orders found for the customer.");
                        }
                    }
                }
            }
        }

        public async Task ProcessAllOrdersAsync(DataGrid dataGridQueue)
        {
            while (true) // Sıradaki tüm müşteriler işlenene kadar
            {
                var nextCustomer = GetNextCustomer();
                if (nextCustomer == null)
                {
                    // Sırada müşteri kalmadığında döngüyü kır
                    break;
                }

                using (var connection = DatabaseHelper.GetConnection())
                {
                    await connection.OpenAsync();

                    string query = @"
            SELECT o.OrderID, o.ProductID, o.Quantity
            FROM Orders o
            WHERE o.CustomerID = @CustomerID AND o.OrderStatus = 'Pending'
            ORDER BY o.OrderDate ASC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CustomerID", nextCustomer.CustomerID);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int orderId = Convert.ToInt32(reader["OrderID"]);
                                int productId = Convert.ToInt32(reader["ProductID"]);
                                int quantity = Convert.ToInt32(reader["Quantity"]);

                                try
                                {
                                    // Siparişi onayla
                                    await ApproveOrderAsync(nextCustomer.CustomerID, orderId, productId, quantity);

                                    // DataGrid'i güncelle
                                    LoadQueueData(dataGridQueue);

                                    // İşlemden sonra bir süre bekle
                                    await Task.Delay(5000);

                                    // Siparişi sil
                                    await DeleteCompletedOrderAsync(orderId);

                                    // DataGrid'i tekrar güncelle
                                    LoadQueueData(dataGridQueue);
                                }
                                catch (Exception ex)
                                {
                                    _logService.AddLog(nextCustomer.CustomerID, orderId, "Error", $"Error processing order: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }

        public void LoadQueueData(DataGrid dataGridQueue)
        {
            string query = @"
        SELECT o.OrderID, o.CustomerID, o.ProductID, o.Quantity, o.OrderStatus
        FROM Orders o
        WHERE o.OrderStatus IN ('Pending', 'Completed')
        ORDER BY o.OrderDate ASC";

            try
            {
                var dataTable = DatabaseHelper.ExecuteQuery(query);
                dataGridQueue.ItemsSource = dataTable.DefaultView; // DataGrid'e veriyi bağla
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading queue data: {ex.Message}");
            }
        }


        public async Task DeleteCompletedOrderAsync(int orderId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();

                string deleteQuery = "DELETE FROM Orders WHERE OrderID = @OrderID AND OrderStatus = 'Completed'";

                using (var command = new MySqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@OrderID", orderId);
                    await command.ExecuteNonQueryAsync();

                    // Loglama
                    _logService.AddLog(0, orderId, "Info", "Order deleted after completion.");
                }
            }
        }

        public async Task DeleteCompletedOrdersAsync()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();

                string selectQuery = "SELECT OrderID, CustomerID FROM Orders WHERE OrderStatus = 'Completed'";
                var orderList = new List<(int OrderId, int CustomerId)>();

                using (var selectCommand = new MySqlCommand(selectQuery, connection))
                using (var reader = await selectCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int orderId = Convert.ToInt32(reader["OrderID"]);
                        int customerId = Convert.ToInt32(reader["CustomerID"]);
                        orderList.Add((orderId, customerId));
                    }
                }

                string deleteQuery = "DELETE FROM Orders WHERE OrderStatus = 'Completed'";
                using (var deleteCommand = new MySqlCommand(deleteQuery, connection))
                {
                    await deleteCommand.ExecuteNonQueryAsync();
                }

                // Log ekleme
                foreach (var order in orderList)
                {
                    _logService.AddLog(order.CustomerId, order.OrderId, "Info", "Order deleted after completion.");
                }
            }
        }

        public async Task ProcessAllOrdersWithDelayAsync(DataGrid dataGridQueue, int delayInMilliseconds = 5000)
        {
            while (true) // Sıradaki tüm müşteriler işlenene kadar
            {
                var nextCustomer = GetNextCustomer();
                if (nextCustomer == null)
                {
                    // Sırada müşteri kalmadığında döngüyü kır
                    break;
                }

                using (var connection = DatabaseHelper.GetConnection())
                {
                    await connection.OpenAsync();

                    string query = @"
            SELECT o.OrderID, o.ProductID, o.Quantity
            FROM Orders o
            WHERE o.CustomerID = @CustomerID AND o.OrderStatus = 'Pending'
            ORDER BY o.OrderDate ASC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CustomerID", nextCustomer.CustomerID);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int orderId = Convert.ToInt32(reader["OrderID"]);
                                int productId = Convert.ToInt32(reader["ProductID"]);
                                int quantity = Convert.ToInt32(reader["Quantity"]);

                                try
                                {
                                    // Siparişi onayla
                                    await ApproveOrderAsync(nextCustomer.CustomerID, orderId, productId, quantity);

                                    // DataGrid'i güncelle
                                    LoadQueueData(dataGridQueue);

                                    // İşlemden sonra bir süre bekle
                                    await Task.Delay(delayInMilliseconds);

                                    // Siparişi sil
                                    await DeleteCompletedOrderAsync(orderId);

                                    // DataGrid'i tekrar güncelle
                                    LoadQueueData(dataGridQueue);
                                }
                                catch (Exception ex)
                                {
                                    _logService.AddLog(nextCustomer.CustomerID, orderId, "Error", $"Error processing order: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }



        public async Task ApproveOrderAsync(int customerId, int orderId, int productId, int quantity)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // Sipariş durumunu güncelle
                        string updateOrderStatusQuery = "UPDATE Orders SET OrderStatus = 'Completed' WHERE OrderID = @OrderID";
                        var updateOrderCommand = new MySqlCommand(updateOrderStatusQuery, connection, transaction);
                        updateOrderCommand.Parameters.AddWithValue("@OrderID", orderId);
                        await updateOrderCommand.ExecuteNonQueryAsync();

                        // Log kaydı ekle
                        _logService.AddLog(customerId, orderId, "Info", "Order approved successfully.");

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logService.AddLog(customerId, orderId, "Error", $"Error approving order: {ex.Message}");
                        throw new Exception($"Error approving order: {ex.Message}");
                    }
                }
            }
        }


        public List<Customer> GetAllCustomersInQueue()
        {
            return customerQueue.UnorderedItems
                                .Select(item => item.Element)
                                .ToList();
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
