using StockApp.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StockApp
{
    /// <summary>
    /// Interaction logic for AdminPanel.xaml
    /// </summary>
    public partial class AdminPanel : Window
    {
        public AdminPanel()
        {
            InitializeComponent();
            LoadDataAsync(); // Asenkron verileri yükle
        }

        private async void AdminPanel_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadDataAsync(); // Verileri yükler
                LoadStockStatus();     // Stok durumu panelini yükler
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading admin panel: {ex.Message}");
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await Task.Run(() => LoadCustomers()); // Müşteri verilerini yükle
                await Task.Run(() => LoadProducts());  // Ürün verilerini yükle
                await Task.Run(() => LoadStockStatus());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        // LoadCustomers Method
        private async void LoadCustomers()
        {
            try
            {
                string query = "SELECT CustomerID, CustomerName, Budget, CustomerType, TotalSpent FROM Customers";
                DataTable customers = await Task.Run(() => DatabaseHelper.ExecuteQuery(query));

                // UI iş parçacığına dönerek UI elementlerini güncelle
                Application.Current.Dispatcher.Invoke(() =>
                {
                    dataGridCustomers.ItemsSource = customers.DefaultView;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}");
            }
        }


        // Load Customers Button Click Event
        private void LoadCustomers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadCustomers(); // LoadCustomers methodunu çağırıyoruz.
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}");
            }
        }

        // Load Products for Product Management Tab
        private async void LoadProducts()
        {
            try
            {
                string query = "SELECT * FROM Products";
                DataTable products = await Task.Run(() => DatabaseHelper.ExecuteQuery(query));

                // UI iş parçacığına dönerek UI elementlerini güncelle
                Application.Current.Dispatcher.Invoke(() =>
                {
                    dataGridProducts.ItemsSource = products.DefaultView;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}");
            }
        }


        private void LoadProducts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadProducts(); // Ürün listesini yenilemek için LoadProducts çağrılır.
                LoadStockStatus(); // Stok durumu tablosu ve grafiği yüklenir.
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}");
            }
        }


        public void AddProduct(string productName, int stock, decimal price)
        {
            string query = $"INSERT INTO Products (ProductName, Stock, Price) VALUES ('{productName}', {stock}, {price})";
            DatabaseHelper.ExecuteNonQueryAsync(query);
            MessageBox.Show("Product added successfully.");
            LoadProducts();
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductName.Text) ||
            !int.TryParse(txtStock.Text, out int stock) ||
            !decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Please fill out all fields correctly.");
                return;
            }
            string query = $"INSERT INTO Products (ProductName, Stock, Price) VALUES ('{txtProductName.Text}', {stock}, {price})";
            DatabaseHelper.ExecuteNonQueryAsync(query);
            //AddProduct(txtProductName.Text, stock, price);
            LoadProducts(); // Ürün listesini yenile
        }

        public void UpdateProduct(int productId, int newStock, decimal newPrice)
        {
            string query = $"UPDATE Products SET Stock = {newStock}, Price = {newPrice} WHERE ProductID = {productId}";
            DatabaseHelper.ExecuteNonQueryAsync(query);
            MessageBox.Show("Product updated successfully.");
        }

        public void DeleteProduct(int productId)
        {
            try
            {
                string query = $"DELETE FROM Products WHERE ProductID = {productId}";
                DatabaseHelper.ExecuteNonQueryAsync(query);
                MessageBox.Show("Product deleted successfully.");
                LoadProducts(); // Ürün listesini yenile
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting product: {ex.Message}");
            }
        }

        private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtProductId.Text, out int productId))
            {
                DeleteProduct(productId);
            }
            else
            {
                MessageBox.Show("Please enter a valid Product ID.");
            }
        }


        private void btnUpdateProduct_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtProductId.Text, out int productId) ||
            !int.TryParse(txtStock.Text, out int newStock) ||
            !decimal.TryParse(txtPrice.Text, out decimal newPrice))
            {
                MessageBox.Show("Please fill out all fields correctly.");
                return;
            }
            UpdateProduct(productId, newStock, newPrice);
            LoadProducts();
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            {
                string query = $"UPDATE Orders SET OrderStatus = '{status}' WHERE OrderID = {orderId}";
                DatabaseHelper.ExecuteNonQueryAsync(query);
                MessageBox.Show($"Order status updated to {status}.");
            }
        }

        private void btnApproveOrder_Click(object sender, RoutedEventArgs e)
        {
            int orderId = int.Parse(txtOrderId.Text);
            UpdateOrderStatus(orderId, "Completed");
            LoadOrders();
        }

        private void btnCancelOrder_Click(object sender, RoutedEventArgs e)
        {
            int orderId = int.Parse(txtOrderId.Text);
            UpdateOrderStatus(orderId, "Cancelled");
            LoadOrders();
        }

        private void LoadOrders()
        {
            string query = "SELECT * FROM Orders";
            var orders = DatabaseHelper.ExecuteQuery(query);
            dataGridOrders.ItemsSource = orders.DefaultView;
        }


        private void LoadOrders_Click(object sender, RoutedEventArgs e)
        {
            string query = "SELECT * FROM Orders";
            var orders = DatabaseHelper.ExecuteQuery(query);
            dataGridOrders.ItemsSource = orders.DefaultView;
        }

        private void btnGenerateCustomers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RandomCustomerGenerator.ResetAndGenerateCustomers(5, 10); // 5 ile 10 arasında rastgele müşteri oluştur
                MessageBox.Show("Random customers generated successfully!");
                LoadCustomers(); // Müşteri listesini yenile
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating customers: {ex.Message}");
            }
        }

        private async void LoadStockStatus()
        {
            try
            {
                string stockQuery = "SELECT ProductName AS 'Product', Stock AS 'Quantity' FROM Products";
                DataTable stockTable = await Task.Run(() => DatabaseHelper.ExecuteQuery(stockQuery));

                // UI iş parçacığına dönerek grafiği güncelle
                Application.Current.Dispatcher.Invoke(() =>
                {
                    stockBarChart.Items.Clear();
                    foreach (DataRow row in stockTable.Rows)
                    {
                        int stock = Convert.ToInt32(row["Quantity"]);
                        string productName = row["Product"].ToString();

                        // Bar grafiği oluştur
                        ProgressBar progressBar = new ProgressBar
                        {
                            Width = 200,
                            Height = 20,
                            Value = stock,
                            Maximum = 500,
                            ToolTip = $"{productName} - {stock} units"
                        };

                        TextBlock productLabel = new TextBlock
                        {
                            Text = productName,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        StackPanel stockPanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(5)
                        };

                        stockPanel.Children.Add(productLabel);
                        stockPanel.Children.Add(progressBar);

                        stockBarChart.Items.Add(stockPanel);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading stock status: {ex.Message}");
            }
        }
    }
}
