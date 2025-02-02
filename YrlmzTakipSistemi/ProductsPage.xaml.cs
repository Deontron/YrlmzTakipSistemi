using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        private DatabaseHelper dbHelper;
        private Customer currentCustomer;

        public ProductsPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();

        }

        public void LoadCustomerProducts(Customer customer)
        {
            currentCustomer = customer;

            TitleTextBlock.Text = $"{customer.Name} - Ürünler";
            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Products WHERE CustomerId = @CustomerId";
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerId", customer.Id);

                List<Product> products = new List<Product>();

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1),
                            Tarih = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Isim = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Fiyat = reader.IsDBNull(4) ? 0.0 : reader.GetDouble(4),
                        });
                    }
                }

                ProductsDataGrid.ItemsSource = products;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            TransactionAddPage transactionAddPage = new TransactionAddPage();
            transactionAddPage.GetCustomer(currentCustomer);
            mainWindow.MainFrame.Navigate(transactionAddPage);
        }

        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem != null)
            {
                var selectedProduct = (Product)ProductsDataGrid.SelectedItem;

                var result = MessageBox.Show(selectedProduct.Isim + " Silmek istediğinize emin misiniz?",
                                 "Silme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = DeleteProductFromDatabase(selectedProduct.Id);

                    if (success)
                    {
                        MessageBox.Show("Ürün başarıyla silindi.", "Hop!");
                        LoadCustomerProducts(currentCustomer);
                    }
                    else
                    {
                        MessageBox.Show("Silme işlemi başarısız oldu.", "Hop!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir ürün seçin.", "Hop!");
            }
        }
        private bool DeleteProductFromDatabase(int productId)
        {
            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string deleteProductsQuery = "DELETE FROM Products WHERE Id = @Id";
                        using (var deleteProductsCommand = new SQLiteCommand(deleteProductsQuery, connection, transaction))
                        {
                            deleteProductsCommand.Parameters.AddWithValue("@Id", productId);

                            int rowsAffected = deleteProductsCommand.ExecuteNonQuery();

                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }


        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            ProductsAddPage productAddPage = new ProductsAddPage();
            productAddPage.GetCustomer(currentCustomer);
            mainWindow.MainFrame.Navigate(productAddPage);
        }

        private void UpdateProductButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
