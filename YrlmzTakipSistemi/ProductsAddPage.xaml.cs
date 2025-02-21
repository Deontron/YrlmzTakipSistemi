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
using YrlmzTakipSistemi.Repositories;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for ProductsAddPage.xaml
    /// </summary>
    public partial class ProductsAddPage : Page
    {
        private readonly ProductRepository _productRepository;
        private DatabaseHelper _dbHelper;
        private Customer _currentCustomer;
        public ProductsAddPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _productRepository = new ProductRepository(_dbHelper.GetConnection());
        }

        public void GetCustomer(Customer customer)
        {
            _currentCustomer = customer;
            TitleTextBlock.Text = $"{_currentCustomer.Name} - Yeni Ürün Ekle";
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

            try
            {
                var product = new Product
                {
                    CustomerId = _currentCustomer.Id,
                    Isim = name,
                    Fiyat = amount
                };

                _productRepository.Add(product);

            MessageBox.Show($"{name} başarıyla eklendi!", "Hop!");
                ClearForm();
                Back();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün kaydedilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            productsPage.LoadCustomerProducts(_currentCustomer);
            mainWindow.MainFrame.Navigate(productsPage);
        }
    }
}
