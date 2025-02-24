using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using YrlmzTakipSistemi.Repositories;

namespace YrlmzTakipSistemi
{
    public partial class TransactionAddPage : Page
    {
        private readonly TransactionRepository _transactionRepository;
        private readonly FinancialRepository _financialRepository;
        private readonly ProductRepository _productRepository;
        private readonly CustomerRepository _customerRepository;
        private Customer _currentCustomer;
        public ObservableCollection<Product> Products { get; set; }

        public TransactionAddPage()
        {
            InitializeComponent();
            var _dbHelper = new DatabaseHelper();
            _transactionRepository = new TransactionRepository(_dbHelper.GetConnection());
            _financialRepository = new FinancialRepository(_dbHelper.GetConnection());
            _productRepository = new ProductRepository(_dbHelper.GetConnection());
            _customerRepository = new CustomerRepository(_dbHelper.GetConnection());
            DataContext = this;
            Products = new ObservableCollection<Product>();
        }

        public void GetCustomer(Customer customer)
        {
            _currentCustomer = customer;
            TitleTextBlock.Text = $"{_currentCustomer.Name} - Yeni İşlem Ekle";
            LoadProducts();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToTransactionsPage();
        }

        private void SaveTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs(out string description, out string note, out double price, out int quantity, out double amount, out double paid))
            {
                return;
            }

            try
            {
                amount = amount != 0 ? amount : price * quantity;

                var transaction = new Transaction
                {
                    CustomerId = _currentCustomer.Id,
                    FaturaId = 0,
                    OdemeId = 0,
                    Aciklama = description,
                    Notlar = note,
                    Adet = quantity,
                    BirimFiyat = price,
                    Tutar = amount,
                    Odenen = paid
                };

                int? financialId = null;
                if(amount != 0)
                {
                    var financial = new FinancialTransaction
                    {
                        Aciklama = description,
                        Tutar = amount
                    };

                   financialId = (int?) _financialRepository.Add(financial);
                }

                if (financialId.HasValue)
                {
                    transaction.FinansalId = financialId.Value;
                }
                _transactionRepository.Add(transaction);

                UpdateCustomerDebt(amount);

                if (SaveProductBox.IsChecked == true)
                {
                    SaveProduct(description, price);
                }

                MessageBox.Show($"İşlem {description} başarıyla eklendi!", "Başarılı");
                ClearForm();
                NavigateToTransactionsPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İşlem kaydedilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs(out string description, out string note, out double price, out int quantity, out double amount, out double paid)
        {
            description = DescriptionTextBox.Text;
            note = NoteTextBox.Text;

            price = string.IsNullOrEmpty(PriceTextBox.Text) ? 0 : double.TryParse(PriceTextBox.Text, out double parsedPrice) ? parsedPrice : -1;
            quantity = string.IsNullOrEmpty(QuantityTextBox.Text) ? 0 : int.TryParse(QuantityTextBox.Text, out int parsedQuantity) ? parsedQuantity : -1;
            amount = string.IsNullOrEmpty(AmountTextBox.Text) ? 0 : double.TryParse(AmountTextBox.Text, out double parsedAmount) ? parsedAmount : -1;
            paid = string.IsNullOrEmpty(PaidTextBox.Text) ? 0 : double.TryParse(PaidTextBox.Text, out double parsedPaid) ? parsedPaid : -1;

            if (string.IsNullOrEmpty(description))
            {
                MessageBox.Show("İşlem Açıklaması boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (price == -1)
            {
                MessageBox.Show("Fiyat değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (quantity == -1)
            {
                MessageBox.Show("Adet değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (amount == -1)
            {
                MessageBox.Show("Tutar değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (paid == -1)
            {
                MessageBox.Show("Ödenen değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
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
                var products = _productRepository.GetAll($"CustomerId = {_currentCustomer.Id}");

                foreach (var product in products)
                {
                    DateTime parsedDate;
                    if (DateTime.TryParse(product.Tarih, out parsedDate))
                    {
                        product.Tarih = parsedDate.ToString("dd.MM.yyyy");
                    }
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürünler yüklenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProductSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductListBox.SelectedItem is Product selectedProduct)
            {
                DescriptionTextBox.Text = selectedProduct.Isim;
                PriceTextBox.Text = selectedProduct.Fiyat.ToString();
            }
        }

        private void SaveProduct(string name, double price)
        {
            try
            {
                var product = new Product
                {
                    CustomerId = _currentCustomer.Id,
                    Isim = name,
                    Fiyat = price
                };

                _productRepository.Add(product);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün kaydedilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageProductsButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            var productsPage = new ProductsPage();
            productsPage.LoadCustomerProducts(_currentCustomer);
            mainWindow.MainFrame.Navigate(productsPage);
        }

        private void UpdateCustomerDebt(double amount)
        {
            _customerRepository.UpdateCustomerDebtById(amount, _currentCustomer.Id);
        }

        private void NavigateToTransactionsPage()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            var transactionsPage = new TransactionsPage();
            transactionsPage.LoadCustomerTransactions(_currentCustomer);
            mainWindow.MainFrame.Navigate(transactionsPage);
        }
    }
}