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


        // Sipariş oluşturma
        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ürün seçimi ve miktar girişini doğrula
                if (comboBoxProducts.SelectedItem == null)
                {
                    MessageBox.Show("Please select a product.");
                    return;
                }

                if (!int.TryParse(textBoxQuantity.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Please enter a valid quantity greater than zero.");
                    return;
                }

                string selectedProductName = comboBoxProducts.SelectedItem.ToString();

                // Ürün bilgilerini veritabanından alın
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

                // Stok ve bütçe kontrolü
                if (quantity > availableStock)
                {
                    MessageBox.Show("Insufficient stock for the selected product.");
                    return;
                }

                if (totalCost > _loggedInCustomer.Budget)
                {
                    MessageBox.Show("Insufficient budget.");
                    return;
                }

                // Müşteriyi sıraya ekle ve dinamik öncelik güncellemesi yap
                _loggedInCustomer.OrderTime = DateTime.Now; // Sipariş zamanını ayarla
                _orderService.AddCustomerToQueue(_loggedInCustomer);
                _orderService.UpdateCustomerPriorities();

                // En yüksek öncelikli müşteriyi alın ve işlemi gerçekleştirin
                Customer nextCustomer = _orderService.GetNextCustomer();
                if (nextCustomer != null && nextCustomer.CustomerID == _loggedInCustomer.CustomerID)
                {
                    // Siparişi gerçekleştir
                    _orderService.PurchaseProductAsync(nextCustomer.CustomerID, productId, quantity);

                    // Bütçe ve ürün listesini güncelle
                    _loggedInCustomer.Budget -= totalCost;
                    textBlockCustomerBudget.Text = $"{_loggedInCustomer.Budget:C}";
                    LoadProducts();

                    MessageBox.Show("Purchase successful!");
                }
                else
                {
                    MessageBox.Show("Your order is in the queue. Please wait for processing.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while processing the order: {ex.Message}");
            }
        }


    }
}
