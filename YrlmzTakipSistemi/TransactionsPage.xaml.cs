using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data.SQLite;
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
        private readonly InvoiceRepository _invoiceRepository;
        private readonly PaymentRepository _paymentRepository;
        private readonly CustomerRepository _customerRepository;
        private Customer _currentCustomer;
        private ObservableCollection<Transaction> _transactions = new ObservableCollection<Transaction>();
        private DatabaseHelper dbHelper;
        private PrintHelper printHelper;

        public TransactionsPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            printHelper = new PrintHelper();
            _transactionRepository = new TransactionRepository(dbHelper.GetConnection());
            _invoiceRepository = new InvoiceRepository(dbHelper.GetConnection());
            _paymentRepository = new PaymentRepository(dbHelper.GetConnection());
            _customerRepository = new CustomerRepository(dbHelper.GetConnection());
        }

        public void LoadCustomerTransactions(Customer customer)
        {
            _transactions.Clear();
            _currentCustomer = customer;

            TitleTextBlock.Text = $"{customer.Name} - İşlemler";

            var transactions = _transactionRepository.GetByCustomerId(customer.Id);
            foreach (var transaction in transactions)
            {
                DateTime parsedDate;
                if (DateTime.TryParse(transaction.Tarih, out parsedDate))
                {
                    transaction.Tarih = parsedDate.ToString("dd.MM.yyyy");
                }
                _transactions.Add(transaction);
            }

            TransactionsDataGrid.ItemsSource = _transactions;
            UpdateTotalAmount();
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
            SumTextBlock.Text = $"Toplam Alacak: {totalAmount:N2} TL";
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
    }
}
