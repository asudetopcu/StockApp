using MySql.Data.MySqlClient;
using StockApp.Data;
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
        public void PurchaseProduct(int customerId, int productId, int quantity)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Stok kontrolü
                        string checkStockQuery = $"SELECT Stock FROM Products WHERE ProductID = {productId} FOR UPDATE";
                        MySqlCommand checkStockCommand = new MySqlCommand(checkStockQuery, connection, transaction);
                        int stock = Convert.ToInt32(checkStockCommand.ExecuteScalar());

                        if (stock < quantity)
                        {
                            throw new Exception("Insufficient stock!");
                        }

                        // Ürün fiyatını al
                        string getPriceQuery = $"SELECT Price FROM Products WHERE ProductID = {productId}";
                        MySqlCommand getPriceCommand = new MySqlCommand(getPriceQuery, connection, transaction);
                        decimal price = Convert.ToDecimal(getPriceCommand.ExecuteScalar());

                        decimal totalCost = price * quantity;

                        // Müşteri bakiyesi kontrolü
                        string checkBudgetQuery = $"SELECT Budget FROM Customers WHERE CustomerID = {customerId} FOR UPDATE";
                        MySqlCommand checkBudgetCommand = new MySqlCommand(checkBudgetQuery, connection, transaction);
                        decimal budget = Convert.ToDecimal(checkBudgetCommand.ExecuteScalar());

                        if (budget < totalCost)
                        {
                            throw new Exception("Insufficient budget!");
                        }

                        // Stok güncelleme
                        string updateStockQuery = $"UPDATE Products SET Stock = Stock - {quantity} WHERE ProductID = {productId}";
                        MySqlCommand updateStockCommand = new MySqlCommand(updateStockQuery, connection, transaction);
                        updateStockCommand.ExecuteNonQuery();

                        // Sipariş kaydı
                        string insertOrderQuery = "INSERT INTO Orders (CustomerID, ProductID, Quantity, TotalPrice, OrderDate, OrderStatus) " +
                                                  "VALUES (@CustomerID, @ProductID, @Quantity, @TotalPrice, NOW(), @OrderStatus)";
                        MySqlCommand insertOrderCommand = new MySqlCommand(insertOrderQuery, connection, transaction);
                        insertOrderCommand.Parameters.AddWithValue("@CustomerID", customerId);
                        insertOrderCommand.Parameters.AddWithValue("@ProductID", productId);
                        insertOrderCommand.Parameters.AddWithValue("@Quantity", quantity);
                        insertOrderCommand.Parameters.AddWithValue("@TotalPrice", totalCost.ToString(CultureInfo.InvariantCulture));
                        insertOrderCommand.Parameters.AddWithValue("@OrderStatus", "Completed");
                        insertOrderCommand.ExecuteNonQuery();

                        // Müşteri bakiyesi güncelleme
                        string updateBudgetQuery = $"UPDATE Customers SET Budget = Budget - {totalCost.ToString(CultureInfo.InvariantCulture)}, TotalSpent = TotalSpent + {totalCost.ToString(CultureInfo.InvariantCulture)} WHERE CustomerID = {customerId}";
                        MySqlCommand updateBudgetCommand = new MySqlCommand(updateBudgetQuery, connection, transaction);
                        updateBudgetCommand.ExecuteNonQuery();

                        // Transaction'u onayla
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Purchase failed: {ex.Message}");
                    }
                }
            }
        }




    }
}
