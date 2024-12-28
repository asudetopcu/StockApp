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
            try
            {
                string query = "SELECT CustomerID, CustomerName, Budget, CustomerType, TotalSpent FROM Customers";
                DataTable customers = DatabaseHelper.ExecuteQuery(query);
                dataGridCustomers.ItemsSource = customers.DefaultView;

                // Seçili müşterinin bakiyesini göster
                dataGridCustomers.SelectionChanged += (s, e) =>
                {
                    if (dataGridCustomers.SelectedItem != null)
                    {
                        DataRowView selectedCustomer = (DataRowView)dataGridCustomers.SelectedItem;
                        decimal budget = Convert.ToDecimal(selectedCustomer["Budget"]);
                        MessageBox.Show($"Current Budget: {budget:C}");
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}");
            }
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

                string productQuery = $"SELECT ProductID, Stock, Price FROM Products WHERE ProductName = '{productName}'";
                DataTable productData = DatabaseHelper.ExecuteQuery(productQuery);

                if (productData.Rows.Count == 0)
                {
                    MessageBox.Show("Selected product does not exist.");
                    return;
                }

                int productId = Convert.ToInt32(productData.Rows[0]["ProductID"]);
                int stock = Convert.ToInt32(productData.Rows[0]["Stock"]);
                decimal price = Convert.ToDecimal(productData.Rows[0]["Price"]);
                decimal totalCost = price * quantity;

                if (quantity > stock)
                {
                    MessageBox.Show("Insufficient stock for the selected product.");
                    return;
                }

                int customerId = GetCurrentCustomerId();

                // Sipariş oluştur
                _orderService.PurchaseProduct(customerId, productId, quantity);
                MessageBox.Show("Purchase successful!");

                // Ürün listesini güncelle
                LoadProducts();

                // Bütçeyi güncelle ve kullanıcıya göster
                if (dataGridCustomers.SelectedItem != null)
                {
                    DataRowView selectedCustomer = (DataRowView)dataGridCustomers.SelectedItem;
                    decimal updatedBudget = Convert.ToDecimal(selectedCustomer["Budget"]) - totalCost;
                    selectedCustomer["Budget"] = updatedBudget; // GridView'deki bütçeyi güncelle
                    textBlockCustomerBudget.Text = $"Current Budget: {updatedBudget:C}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }



        private int GetCurrentCustomerId()
        {
            // Geçerli bir müşteri seçimi olup olmadığını kontrol edin
            if (dataGridCustomers.SelectedItem == null)
            {
                throw new InvalidOperationException("Please select a customer from the list.");
            }

            // Seçilen müşteri ID'sini döndür
            DataRowView selectedCustomer = (DataRowView)dataGridCustomers.SelectedItem;
            return Convert.ToInt32(selectedCustomer["CustomerID"]);
        }

    }
}
