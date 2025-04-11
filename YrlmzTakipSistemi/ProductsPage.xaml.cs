using System.Windows;
using System.Windows.Controls;
using YrlmzTakipSistemi.Repositories;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        private readonly ProductRepository _productRepository;
        private DatabaseHelper _dbHelper;
        private Customer _currentCustomer;
        private PrintHelper printHelper;
        public ProductsPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            printHelper = new PrintHelper();
            _productRepository = new ProductRepository(_dbHelper.GetConnection());
        }

        public void LoadCustomerProducts(Customer customer)
        {
            _currentCustomer = customer;

            TitleTextBlock.Text = $"{customer.Name} - Ürünler";

            List<Product> _products = new List<Product>();
            var products = _productRepository.GetByCustomerId(customer.Id);
            foreach (var transaction in products)
            {
                _products.Add(transaction);
            }

            ProductsDataGrid.ItemsSource = _products;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            TransactionAddPage transactionAddPage = new TransactionAddPage();
            transactionAddPage.GetCustomer(_currentCustomer);
            mainWindow.MainFrame.Navigate(transactionAddPage);
        }

        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show(selectedProduct.Isim + " Silmek istediğinize emin misiniz?",
                                 "Silme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _productRepository.Delete(selectedProduct.Id);
                    MessageBox.Show("Ürün başarıyla silindi.", "Hop!");
                    LoadCustomerProducts(_currentCustomer);
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir ürün seçin.", "Hop!");
            }
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            ProductsAddPage productAddPage = new ProductsAddPage();
            productAddPage.GetCustomer(_currentCustomer);
            mainWindow.MainFrame.Navigate(productAddPage);
        }

        private void UpdateProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show(selectedProduct.Isim + " Güncellemek istediğinize emin misiniz?",
                                 "Güncelleme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _productRepository.Update(selectedProduct);
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir ürün seçin.", "Hop!");
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            printHelper.PrintDataGrid(ProductsDataGrid, "Ürünler");
        }
    }
}
