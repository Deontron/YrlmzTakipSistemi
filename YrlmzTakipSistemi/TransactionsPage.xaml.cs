using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using YrlmzTakipSistemi.Repositories;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for TransactionsPage.xaml
    /// </summary>
    public partial class TransactionsPage : Page
    {
        private readonly TransactionRepository _transactionRepository;
        private readonly FinancialRepository _financialRepository;
        private readonly InvoiceRepository _invoiceRepository;
        private readonly PaymentRepository _paymentRepository;
        private readonly CustomerRepository _customerRepository;
        private Customer _currentCustomer;
        private ObservableCollection<Transaction> _transactions = new ObservableCollection<Transaction>();
        private List<Transaction> filteredTransactions = new List<Transaction>();
        private DatabaseHelper _dbHelper;
        private PrintHelper printHelper;

        public TransactionsPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            printHelper = new PrintHelper();
            _transactionRepository = new TransactionRepository(_dbHelper.GetConnection());
            _invoiceRepository = new InvoiceRepository(_dbHelper.GetConnection());
            _paymentRepository = new PaymentRepository(_dbHelper.GetConnection());
            _customerRepository = new CustomerRepository(_dbHelper.GetConnection());
            _financialRepository = new FinancialRepository(_dbHelper.GetConnection());
        }

        public void LoadCustomerTransactions(Customer customer)
        {
            _transactions.Clear();
            _currentCustomer = customer;

            TitleTextBlock.Text = $"{customer.Name}";

            var transactions = _transactionRepository.GetByCustomerId(customer.Id);

            double cumulativeTotal = 0;

            foreach (var transaction in transactions)
            {
                cumulativeTotal += (transaction.Tutar - transaction.Odenen);
                transaction.KumulatifAlacak = cumulativeTotal;

                _transactions.Add(transaction);
            }

            TransactionsDataGrid.ItemsSource = _transactions;
            UpdateTotalAmount();
            SetDateFilter();
        }


        private void AddTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            TransactionAddPage transactionAddPage = new TransactionAddPage();
            transactionAddPage.GetCustomer(_currentCustomer);
            mainWindow.MainFrame.Navigate(transactionAddPage);
        }

        private void DeleteTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem is Transaction selectedTransaction)
            {
                var result = MessageBox.Show($"{selectedTransaction.Aciklama} Silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _customerRepository.UpdateCustomerDebtById(-selectedTransaction.AlacakDurumu, _currentCustomer.Id);
                    _transactionRepository.Delete(selectedTransaction.Id);
                    if (selectedTransaction.FaturaId != 0)
                    {
                        _invoiceRepository.Delete((int)selectedTransaction.FaturaId);
                    }
                    else if (selectedTransaction.OdemeId != 0)
                    {
                        _paymentRepository.Delete((int)selectedTransaction.OdemeId);
                    }

                    if (selectedTransaction.FinansalId != 0)
                    {
                        _financialRepository.Delete((int)selectedTransaction.FinansalId);
                    }
                    MessageBox.Show("İşlem başarıyla silindi.", "Bilgi");
                    LoadCustomerTransactions(_currentCustomer);
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir işlem seçin.", "Uyarı");
            }
        }

        private void UpdateTotalAmount()
        {
            double totalAmount = _transactionRepository.GetTotalDebtByCustomerId(_currentCustomer.Id);
            SumTextBlock.Text = $"Bakiye: {totalAmount:C}";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new CustomersPage());
        }
        private void AddPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            PaymentAddPage pp = new PaymentAddPage();
            pp.GetCustomer(_currentCustomer);
            mainWindow.MainFrame.Navigate(pp);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            var filteredCustomers = _transactions.Where(c => c.Aciklama.ToLower().Contains(searchText)).ToList();

            TransactionsDataGrid.ItemsSource = filteredCustomers;
        }

        private void TransactionsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Transaction transaction = e.Row.Item as Transaction;

            if (transaction != null)
            {
                if (transaction.Odenen > 0)
                {
                    e.Row.Background = new SolidColorBrush(Colors.MediumPurple);
                }
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem is Transaction selectedTransaction)
            {
                var result = MessageBox.Show(selectedTransaction.Aciklama + " Güncellemek istediğinize emin misiniz?",
                                 "Güncelleme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _transactionRepository.Update(selectedTransaction);
                    if (selectedTransaction.FinansalId != 0)
                    {
                        FinancialTransaction financialTransaction = _financialRepository.GetById((int)selectedTransaction.FinansalId);
                        financialTransaction.Tutar = selectedTransaction.Tutar;
                        _financialRepository.Update(financialTransaction);
                    }

                    _customerRepository.UpdateCustomerDebtById(_currentCustomer.Debt, _currentCustomer.Id);
                    LoadCustomerTransactions(_currentCustomer);
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir işlem seçin.", "Hop!");
            }
        }

        private void AddInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            InvoiceAddPage invoiceAddPage = new InvoiceAddPage();
            invoiceAddPage.GetCustomer(_currentCustomer);
            mainWindow.MainFrame.Navigate(invoiceAddPage);
        }

        private void CustomerInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            InfoPage infoPage = new InfoPage();
            infoPage.LoadCustomerInfo(_currentCustomer);
            mainWindow.MainFrame.Navigate(infoPage);
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            printHelper.PrintDataGrid(TransactionsDataGrid, _currentCustomer.Name + " İşlemler");
        }

        private void SetDateFilter()
        {
            if (_transactions == null || !_transactions.Any())
            {
                return;
            }

            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            var years = _transactions
                .Where(t => t.Tarih != null)
                .Select(t => t.Tarih.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            if (years.Count == 0)
            {
                return;
            }

            YearComboBox.Items.Clear();
            foreach (var year in years)
            {
                YearComboBox.Items.Add(year.ToString());
            }

            int yearIndex = years.IndexOf(currentYear);
            YearComboBox.SelectedIndex = yearIndex >= 0 ? yearIndex : 0;

            foreach (ComboBoxItem item in MonthComboBox.Items)
            {
                if (item.Tag != null && int.TryParse(item.Tag.ToString(), out int month) && month == currentMonth)
                {
                    MonthComboBox.SelectedItem = item;
                    break;
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetDateFilter();
        }

        private void FilterTransactions(object sender, RoutedEventArgs e)
        {
            int selectedYear = DateTime.Now.Year;
            if (YearComboBox.SelectedItem != null && int.TryParse(YearComboBox.SelectedItem.ToString(), out int year))
            {
                selectedYear = year;
            }

            int selectedMonth = 0;
            if (MonthComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null && int.TryParse(selectedItem.Tag.ToString(), out int month))
            {
                selectedMonth = month;
            }

            if (_transactions == null)
            {
                TransactionsDataGrid.ItemsSource = null;
                return;
            }

            filteredTransactions = _transactions
                .Where(t => t.Tarih.Year == selectedYear && (selectedMonth == 0 || t.Tarih.Month == selectedMonth))
                .ToList();

            TransactionsDataGrid.ItemsSource = filteredTransactions;
        }

        private void LoadAllButton_Click(object sender, RoutedEventArgs e)
        {
            TransactionsDataGrid.ItemsSource = _transactions;
        }
    }
}
