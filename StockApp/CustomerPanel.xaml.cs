using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using StockApp.Data;
using StockApp.Services;

namespace StockApp
{
    public partial class CustomerPanel : Window
    {
        private OrderService _orderService;
        public CustomerPanel()
        {
            InitializeComponent();
            LoadCustomers();
            LoadProducts();
            _orderService = new OrderService();
        }

        // Müşteri bilgilerini yükleme
        private void LoadCustomers()
        {
            string query = "SELECT CustomerID, CustomerName, Budget, CustomerType, TotalSpent FROM Customers";
            DataTable customers = DatabaseHelper.ExecuteQuery(query);
            dataGridCustomers.ItemsSource = customers.DefaultView;
        }

        // Ürünleri yükleme
        private void LoadProducts()
        {
            string query = "SELECT ProductName FROM Products WHERE Stock > 0";
            DataTable products = DatabaseHelper.ExecuteQuery(query);

            foreach (DataRow row in products.Rows)
            {
                comboBoxProducts.Items.Add(row["ProductName"].ToString());
            }
        }

        // Sipariş oluşturma
        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBoxProducts.SelectedItem == null || string.IsNullOrWhiteSpace(textBoxQuantity.Text))
                {
                    MessageBox.Show("Please select a product and enter the quantity.");
                    return;
                }

                string productName = comboBoxProducts.SelectedItem.ToString();
                int quantity = int.Parse(textBoxQuantity.Text);

                string query = $"SELECT ProductID FROM Products WHERE ProductName = '{productName}'";
                DataTable productData = DatabaseHelper.ExecuteQuery(query);

                if (productData.Rows.Count > 0)
                {
                    int productId = Convert.ToInt32(productData.Rows[0]["ProductID"]);
                    int customerId = GetCurrentCustomerId();

                    // Ürünü satın al
                    _orderService.PurchaseProduct(customerId, productId, quantity);
                    MessageBox.Show("Purchase successful!");

                    // Ürün listesini güncelle
                    LoadProducts();
                }
                else
                {
                    MessageBox.Show("Product not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private int GetCurrentCustomerId()
        {
            // Giriş yapan müşteri ID'sini döndür
            // Örneğin: Login sırasında giriş yapan müşteri ID'sini bir değişkene kaydedebilirsiniz
            return 1; // Test için sabit bir ID döndürülebilir
        }
    }
}
