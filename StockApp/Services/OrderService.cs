using MySql.Data.MySqlClient;
using StockApp.Data;
using System;
using System.Collections.Generic;
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
                        MySqlCommand checkCommand = new MySqlCommand(checkStockQuery, connection, transaction);
                        int stock = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (stock < quantity)
                        {
                            throw new Exception("Insufficient stock!");
                        }

                        // Stok güncelleme
                        string updateStockQuery = $"UPDATE Products SET Stock = Stock - {quantity} WHERE ProductID = {productId}";
                        MySqlCommand updateCommand = new MySqlCommand(updateStockQuery, connection, transaction);
                        updateCommand.ExecuteNonQuery();

                        // Sipariş kaydı
                        string insertOrderQuery = $"INSERT INTO Orders (CustomerID, ProductID, Quantity, TotalPrice, OrderStatus, OrderDate) " +
                                                  $"VALUES ({customerId}, {productId}, {quantity}, " +
                                                  $"(SELECT Price * {quantity} FROM Products WHERE ProductID = {productId}), 'Completed', NOW())";
                        MySqlCommand insertCommand = new MySqlCommand(insertOrderQuery, connection, transaction);
                        insertCommand.ExecuteNonQuery();

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
