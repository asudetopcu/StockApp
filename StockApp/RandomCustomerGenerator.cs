using System;
using System.Data;

namespace StockApp.Data
{
    public static class RandomCustomerGenerator
    {
        private static Random random = new Random();
        private static HashSet<string> generatedNames = new HashSet<string>(); // Benzersiz isimleri kontrol etmek için HashSet

        public static void ResetAndGenerateCustomers(int minCustomers, int maxCustomers)
        {
                // Veritabanını sıfırla
                ClearCustomersFromDatabase();

                int customerCount = random.Next(minCustomers, maxCustomers + 1); // Rastgele müşteri sayısı
                int premiumCount = 0;

                for (int i = 0; i < customerCount; i++)
                {
                    string customerName;

                    // Benzersiz müşteri ismi oluşturma
                    do
                    {
                        customerName = $"Customer{random.Next(1, 1000)}"; // "Customer" ile başlayan ve 1-999 arasında rastgele sayı
                    } while (generatedNames.Contains(customerName));

                    generatedNames.Add(customerName); // İsmi benzersiz listeye ekle

                    decimal budget = random.Next(500, 3001); // 500-3000 TL arasında rastgele bütçe
                    string customerType;

                    // En az 2 Premium müşteri olmasını sağlamak
                    if (premiumCount < 2 || (random.NextDouble() < 0.5 && premiumCount < customerCount))
                    {
                        customerType = "Premium";
                        premiumCount++;
                    }
                    else
                    {
                        customerType = "Standard";
                    }

                    // Veritabanına ekle
                    AddCustomerToDatabase(customerName, budget, customerType);
                }
        }

        private static void ClearCustomersFromDatabase()
        {
            string query = "DELETE FROM Customers"; // Müşteri tablosunu temizler
            DatabaseHelper.ExecuteNonQueryAsync(query);
        }

        private static void AddCustomerToDatabase(string customerName, decimal budget, string customerType)
        {
            string query = $"INSERT INTO Customers (CustomerName, Password, Budget, CustomerType, TotalSpent, IsAdmin) " +
                           $"VALUES ('{customerName}', 'password', {budget}, '{customerType}', 0, FALSE)";
            DatabaseHelper.ExecuteNonQueryAsync(query);
        }
    }
}
