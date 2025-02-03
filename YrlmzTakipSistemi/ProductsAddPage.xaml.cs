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
    /// Interaction logic for ProductsAddPage.xaml
    /// </summary>
    public partial class ProductsAddPage : Page
    {
        private DatabaseHelper dbHelper;
        private Customer currentCustomer;
        public ProductsAddPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
        }

        public void GetCustomer(Customer customer)
        {
            currentCustomer = customer;
            TitleTextBlock.Text = $"{currentCustomer.Name} - Yeni Ürün Ekle";
        }

        private void SaveProductButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            double amount = 0;

            if (!string.IsNullOrEmpty(AmountTextBox.Text))
            {
                if (double.TryParse(AmountTextBox.Text, out amount))
                {
                }
                else
                {
                    MessageBox.Show("Ücret değeri geçersiz");
                    return;
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Ürün ismi boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string insertQuery = "INSERT INTO Products (CustomerId, Isim, Fiyat) VALUES (@CustomerId, @Name, @Price)";
                var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Price", amount);


                command.ExecuteNonQuery();
            }

            MessageBox.Show($"İşlem {name} başarıyla eklendi!", "Hop!");
            ClearForm();
            Back();
        }

        private void ClearForm()
        {
            NameTextBox.Clear();
            AmountTextBox.Clear();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }

        private void Back()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            ProductsPage productsPage = new ProductsPage();
            productsPage.LoadCustomerProducts(currentCustomer);
            mainWindow.MainFrame.Navigate(productsPage);
        }
    }
}
