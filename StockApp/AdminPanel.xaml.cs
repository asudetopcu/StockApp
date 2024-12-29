using StockApp.Data;
using StockApp.Models;
using StockApp.Services;
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
        private readonly OrderService _orderService = new OrderService();
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
                // Admin müşteriyi dışarıda bırakan sorgu
                string query = "SELECT CustomerID, CustomerName, Budget, CustomerType, TotalSpent FROM Customers WHERE CustomerName != 'Admin'";
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

        public void LoadLogs()
        {
            string query = "SELECT * FROM Logs ORDER BY LogDate DESC";

            try
            {
                DataTable logs = DatabaseHelper.ExecuteQuery(query);
                dataGridLogs.ItemsSource = logs.DefaultView; // DataGrid'e bağlama
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading logs: {ex.Message}");
            }
        }

        private void LoadLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logService = new LogService();
                logService.LoadLogs(dataGridLogs); // DataGrid'e logları yükle
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading logs: {ex.Message}");
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

        public void ApproveOrder(int orderId)
        {
            string query = $"SELECT CustomerID, OrderDate FROM Orders WHERE OrderID = {orderId}";
            DataTable orderData = DatabaseHelper.ExecuteQuery(query);

            if (orderData.Rows.Count > 0)
            {
                int customerId = Convert.ToInt32(orderData.Rows[0]["CustomerID"]);
                DateTime orderDate = Convert.ToDateTime(orderData.Rows[0]["OrderDate"]);
                int waitTimeInSeconds = (int)(DateTime.Now - orderDate).TotalSeconds;

                // Loglama ve dinamik öncelik güncellemesi
                _orderService.LogOrderProcessing(customerId, orderId, LogType.Approved, "Order approved successfully.");
                _orderService.UpdateCustomerPriorities();
            }
        }


        private async void btnApproveOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Tüm pending siparişleri complete yap
                await _orderService.UpdateOrdersToCompletedAsync();

                // DataGrid'i güncelle
                _orderService.LoadQueueData(dataGridQueue);

                // Complete olan siparişleri sil
                await _orderService.DeleteCompletedOrdersAsync();

                // DataGrid'i yeniden güncelle
                _orderService.LoadQueueData(dataGridQueue);

                // Logs tabını güncelle
                LoadLogs();

                MessageBox.Show("Pending orders have been completed and deleted!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing orders: {ex.Message}");
            }
        }

        private async void ApproveNextOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _orderService.ProcessNextOrderAsync();
                MessageBox.Show("Next order processed successfully!");
                LoadOrders(); // Sipariş listesini güncelle
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }



        private void btnCancelOrder_Click(object sender, RoutedEventArgs e)
        {
            int orderId = int.Parse(txtOrderId.Text);
            UpdateOrderStatus(orderId, "Cancelled");
            LoadOrders();
        }

        private void SortOrders(DataTable orders)
        {
            var sortedOrders = orders.AsEnumerable()
        .OrderBy(row => row["Customer Type"].ToString() == "Premium" ? 0 : 1) // Premium müşteriler önde
        .ThenBy(row => Convert.ToDateTime(row["Order Date"])) // Sipariş tarihi sıralaması
        .CopyToDataTable();

            dataGridQueue.ItemsSource = sortedOrders.DefaultView;
        }

        


        public void LoadOrders()
        {
            try
            {
                string query = @"
           SELECT 
               o.OrderID AS 'Order ID', 
               c.CustomerName AS 'Customer Name', 
               c.CustomerType AS 'Customer Type', 
               p.ProductName AS 'Product Name', 
               o.Quantity AS 'Quantity',
               o.TotalPrice AS 'Total Price',
               o.OrderDate AS 'Order Date', 
               o.OrderStatus AS 'Status'
           FROM Orders o
           JOIN Customers c ON o.CustomerID = c.CustomerID
           JOIN Products p ON o.ProductID = p.ProductID
           ORDER BY 
               CASE WHEN c.CustomerType = 'Premium' THEN 0 ELSE 1 END, 
               o.OrderDate ASC";

                // `orders` değişkenini doldurun
                DataTable orders = DatabaseHelper.ExecuteQuery(query);

                if (orders.Rows.Count == 0)
                {
                    MessageBox.Show("No orders found.");
                    return;
                }

                // DataGrid'e bağla
                dataGridQueue.ItemsSource = orders.DefaultView;
                SortOrders(orders);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}");
            }
        }


        private void LoadOrders_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl)
            {
                // Order Management tabına geçildiğinde siparişleri yükle
                if (tabControl.SelectedItem is TabItem selectedTab && selectedTab.Header.ToString() == "Order Management")
                {
                    LoadOrders();
                }
            }
        }

        private void LoadOrderQueue_Click(object sender, RoutedEventArgs e)
        {
            List<Customer> queue = _orderService.GetAllCustomersInQueue();
            var queueData = queue.Select((customer, index) => new
            {
                Rank = index + 1,
                CustomerID = customer.CustomerID,
                Name = customer.CustomerName,
                Type = customer.CustomerType,
                Budget = customer.Budget,
                OrderTime = customer.OrderTime.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            dataGridQueue.ItemsSource = queueData;
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
        private void LoadQueue()
        {
            var customersInQueue = _orderService.GetAllCustomersInQueue();

            // DataGrid'e bağla
            Application.Current.Dispatcher.Invoke(() =>
            {
                dataGridQueue.ItemsSource = customersInQueue;
            });
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            // Giriş panelini aç
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // Mevcut pencereyi kapat
            this.Close();
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
