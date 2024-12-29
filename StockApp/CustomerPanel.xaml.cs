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
using StockApp.Models;


namespace StockApp
{
    public partial class CustomerPanel : Window
    {
        private readonly Customer _loggedInCustomer;
        private OrderService _orderService;
        public CustomerPanel(Customer loggedInCustomer)
        {
            InitializeComponent();
            _loggedInCustomer = loggedInCustomer;
            _orderService = new OrderService();
            DisplayCustomerInfo();
            LoadProducts();
        }

        private void DisplayCustomerInfo()
        {
            textBlockCustomerName.Text = _loggedInCustomer.CustomerName;
            textBlockCustomerBudget.Text = $"{_loggedInCustomer.Budget:C}";
        }


        // Ürünleri yükleme
        private void LoadProducts()
        {
            try
            {
                string query = "SELECT ProductID, ProductName, Stock, Price FROM Products WHERE Stock > 0";
                DataTable products = DatabaseHelper.ExecuteQuery(query);

                // Populate DataGrid with product details
                dataGridProducts.ItemsSource = products.DefaultView;

                // Populate ComboBox with product names
                comboBoxProducts.Items.Clear();
                foreach (DataRow row in products.Rows)
                {
                    comboBoxProducts.Items.Add(row["ProductName"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}");
            }
        }
        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            // Giriş panelini aç
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // Mevcut pencereyi kapat
            this.Close();
        }


        // Sipariş oluşturma
        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBoxProducts.SelectedItem == null)
                {
                    MessageBox.Show("Please select a product.");
                    return;
                }

                if (!int.TryParse(textBoxQuantity.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Please enter a valid quantity.");
                    return;
                }

                string selectedProductName = comboBoxProducts.SelectedItem.ToString();

                // Ürün bilgilerini al
                string productQuery = $"SELECT ProductID, Stock, Price FROM Products WHERE ProductName = '{selectedProductName}'";
                DataTable productData = DatabaseHelper.ExecuteQuery(productQuery);

                if (productData.Rows.Count == 0)
                {
                    MessageBox.Show("The selected product does not exist.");
                    return;
                }

                int productId = Convert.ToInt32(productData.Rows[0]["ProductID"]);
                int availableStock = Convert.ToInt32(productData.Rows[0]["Stock"]);
                decimal productPrice = Convert.ToDecimal(productData.Rows[0]["Price"]);
                decimal totalCost = productPrice * quantity;

                // Stok kontrolü
                if (quantity > availableStock)
                {
                    MessageBox.Show("Insufficient stock for the selected product.");
                    return;
                }

                // Müşteri bütçesi kontrolü
                if (totalCost > _loggedInCustomer.Budget)
                {
                    MessageBox.Show("Insufficient budget.");
                    return;
                }

                // Siparişi `Orders` tablosuna ekle
                string insertOrderQuery = $@"
            INSERT INTO Orders (CustomerID, ProductID, Quantity, TotalPrice, OrderStatus) 
            VALUES ({_loggedInCustomer.CustomerID}, {productId}, {quantity}, {totalCost}, 'Pending')";
                DatabaseHelper.ExecuteNonQuery(insertOrderQuery);

                // Stoğu güncelle
                string updateStockQuery = $"UPDATE Products SET Stock = Stock - {quantity} WHERE ProductID = {productId}";
                DatabaseHelper.ExecuteNonQuery(updateStockQuery);

                // Müşteri bütçesini güncelle
                _loggedInCustomer.Budget -= totalCost;
                textBlockCustomerBudget.Text = $"{_loggedInCustomer.Budget:C}";

                MessageBox.Show("Order placed successfully.");
                LoadProducts();
                // AdminPanel sınıfındaki LoadOrders metodunu çağır
                AdminPanel adminPanel = new AdminPanel();
                adminPanel.LoadOrders(); // Metodun burada çağrıldığından emin olun
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error placing order: {ex.Message}");
            }

        }
        }
}
