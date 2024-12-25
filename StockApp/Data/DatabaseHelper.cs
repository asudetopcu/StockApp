using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Data;

namespace StockApp.Data
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = "Server=localhost;Database=StockManagement;Uid=root;Pwd=password123;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public static DataTable ExecuteQuery(string query)
        {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var adapter = new MySqlDataAdapter(command))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);
                            return table;
                        }
                    }
                }
        }

        public static async Task<int> ExecuteNonQueryAsync(string query)
        {
                using (var connection = GetConnection())
                {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    return await command.ExecuteNonQueryAsync(); // Etkilenen satır sayısını döndürür
                }
            }
        }
    }
}
