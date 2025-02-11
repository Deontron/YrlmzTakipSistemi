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
using System.Data.Entity.Core.Common.CommandTrees;
using System.Collections.ObjectModel;
using Google.Apis.Drive.v3.Data;
using System.Windows.Controls.Primitives;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for TransactionAddPage.xaml
    /// </summary>
    public partial class TransactionAddPage : Page
    {

        private DatabaseHelper dbHelper;
        Customer currentCustomer;
        public ObservableCollection<Product> Products { get; set; }

        public TransactionAddPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            DataContext = this;
            Products = new ObservableCollection<Product>();
        }

        public void GetCustomer(Customer customer)
        {
            currentCustomer = customer;
            TitleTextBlock.Text = $"{currentCustomer.Name} - Yeni İşlem Ekle";
            LoadProducts();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            TransactionsPage tp = new TransactionsPage();
            tp.LoadCustomerTransactions(currentCustomer);
            mainWindow.MainFrame.Navigate(tp);
        }

        private void SaveTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            string description = DescriptionTextBox.Text;
            string note = NoteTextBox.Text;

            double price = 0;
            int quantity = 0;
            double amount = 0;
            double paid = 0;

            if (!string.IsNullOrEmpty(PriceTextBox.Text))
            {
                if (double.TryParse(PriceTextBox.Text, out price))
                {
                }
                else
                {
                    MessageBox.Show("Fiyat değeri geçersiz");
                    return;
                }
            }

            if (!string.IsNullOrEmpty(QuantityTextBox.Text))
            {
                if (int.TryParse(QuantityTextBox.Text, out quantity))
                {
                }
                else
                {
                    MessageBox.Show("Adet değeri geçersiz");
                    return;
                }
            }

            if (!string.IsNullOrEmpty(AmountTextBox.Text))
            {
                if (double.TryParse(AmountTextBox.Text, out amount))
                {
                }
                else
                {
                    MessageBox.Show("Tutar değeri geçersiz");
                    return;
                }
            }

            if (!string.IsNullOrEmpty(PaidTextBox.Text))
            {
                if (double.TryParse(PaidTextBox.Text, out paid))
                {
                }
                else
                {
                    MessageBox.Show("Ödenen değeri geçersiz");
                    return;
                }
            }

            if (string.IsNullOrEmpty(description))
            {
                MessageBox.Show("İşlem Açıklaması boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string insertQuery = "INSERT INTO Transactions (CustomerId, Aciklama, Notlar, Adet, BirimFiyat, Tutar, Odenen) VALUES (@CustomerId, @Description, @Note, @Quantity, @Price, @Amount, @Paid)";
                var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@Description", description);
                command.Parameters.AddWithValue("@Note", note);
                command.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.Parameters.AddWithValue("@Price", price);

                if (amount != 0)
                {
                    command.Parameters.AddWithValue("@Amount", amount);
                    UpdateCustomerDebt(amount);
                }
                else
                {
                    command.Parameters.AddWithValue("@Amount", (price * quantity));
                    UpdateCustomerDebt((price * quantity));
                }

                command.Parameters.AddWithValue("@Paid", paid);

                command.ExecuteNonQuery();
                connection.Close();
            }

            MessageBox.Show($"İşlem {description} başarıyla eklendi!", "Hop!");

            if (SaveProductBox.IsChecked == true)
            {
                SaveProduct(description, price);
            }

            ClearForm();

            var mainWindow = (MainWindow)Application.Current.MainWindow;
            TransactionsPage transactionsPage = new TransactionsPage();
            transactionsPage.LoadCustomerTransactions(currentCustomer);
            mainWindow.MainFrame.Navigate(transactionsPage);
        }

        private void ClearForm()
        {
            DescriptionTextBox.Clear();
            NoteTextBox.Clear();
            PriceTextBox.Clear();
            QuantityTextBox.Clear();
            AmountTextBox.Clear();
            PaidTextBox.Clear();
        }

        private void LoadProducts()
        {
            try
            {
                Products.Clear();

                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM Products WHERE CustomerId = @CustomerId";

                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tarih = reader.IsDBNull(2) ? string.Empty : reader.GetDateTime(2).ToString("dd-MM-yyyy");
                            string isim = reader.GetString(3);
                            double fiyat = reader.IsDBNull(4) ? 0 : reader.GetDouble(4);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Products.Add(new Product
                                {
                                    Tarih = tarih,
                                    Isim = isim,
                                    Fiyat = fiyat
                                });
                            });
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürünleri yüklerken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProductSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedProduct = (Product)ProductListBox.SelectedItem;

            if (selectedProduct != null)
            {
                DescriptionTextBox.Text = selectedProduct.Isim;

                PriceTextBox.Text = selectedProduct.Fiyat.ToString();
            }
        }

        private void SaveProduct(string Name, double Price)
        {
            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string insertQuery = "INSERT INTO Products (CustomerId, Isim, Fiyat) VALUES (@CustomerId, @Name, @Price)";
                var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                command.Parameters.AddWithValue("@Name", Name);
                command.Parameters.AddWithValue("@Price", Price);


                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        private void ManageProductsButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            ProductsPage productsPage = new ProductsPage();
            productsPage.LoadCustomerProducts(currentCustomer);
            mainWindow.MainFrame.Navigate(productsPage);
        }

        private void UpdateCustomerDebt(double amount)
        {
            double totalDebt = 0;

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string query = "SELECT Debt FROM Customers WHERE Id = @CustomerId";
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);

                var result = command.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    totalDebt = Convert.ToDouble(result);
                }
                connection.Close();
            }

            totalDebt += amount;

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string updateQuery = "UPDATE Customers SET Debt = @Debt WHERE Id = @CustomerId";
                SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@Debt", totalDebt);
                updateCommand.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                updateCommand.ExecuteNonQuery();
                
                connection.Close();
            }
        }
    }
}
