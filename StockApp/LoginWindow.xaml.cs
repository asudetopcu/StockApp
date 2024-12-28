using System;
using System.Data;
using System.Windows;
using StockApp.Models;
using StockApp.Data;

namespace StockApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password; // PasswordBox için

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            // Kullanıcı doğrulama sorgusu
            string query = $"SELECT * FROM Customers WHERE CustomerName = '{username}' AND Password = '{password}'";
            DataTable result = DatabaseHelper.ExecuteQuery(query);

            if (result.Rows.Count > 0)
            {
                DataRow user = result.Rows[0];

                // Kullanıcı bilgilerini al
                Customer loggedInCustomer = new Customer
                {
                    CustomerID = Convert.ToInt32(user["CustomerID"]),
                    CustomerName = user["CustomerName"].ToString(),
                    Password = user["Password"].ToString(),
                    Budget = Convert.ToDecimal(user["Budget"]),
                    CustomerType = user["CustomerType"].ToString(),
                    TotalSpent = Convert.ToInt32(user["TotalSpent"]),
                    IsAdmin = Convert.ToBoolean(user["IsAdmin"])
                };

                if (loggedInCustomer.IsAdmin)
                {
                    // Admin panelini aç
                    AdminPanel adminPanel = new AdminPanel();
                    adminPanel.Show();
                }
                else
                {
                    // Müşteri panelini aç
                    CustomerPanel customerPanel = new CustomerPanel(loggedInCustomer);
                    customerPanel.Show();
                }

                this.Close(); // Login penceresini kapat
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }
    }
}
