using System.Windows;
using System.Windows.Controls;
using YrlmzTakipSistemi.Repositories;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for InvoiceAddPage.xaml
    /// </summary>
    public partial class InvoiceAddPage : Page
    {
        private DatabaseHelper _dbHelper;
        private InvoiceRepository _invoiceRepository;
        private readonly TransactionRepository _transactionRepository;
        private readonly CustomerRepository _customerRepository;
        private readonly FinancialRepository _financialRepository;
        private Customer currentCustomer;
        public InvoiceAddPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _invoiceRepository = new InvoiceRepository(_dbHelper.GetConnection());
            _transactionRepository = new TransactionRepository(_dbHelper.GetConnection());
            _customerRepository = new CustomerRepository(_dbHelper.GetConnection());
            _financialRepository = new FinancialRepository(_dbHelper.GetConnection());
            InvoiceDatePicker.SelectedDate = DateTime.Today;
            KdvTextBox.Text = Settings.Default.Kdv.ToString();
        }

        public void GetCustomer(Customer customer)
        {
            currentCustomer = customer;
            TitleTextBlock.Text = $"{currentCustomer.Name} - Yeni Fatura Ekle";
        }

        private void SaveInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            string invoiceNo = InvoiceNoTextBox.Text;
            double amount = 0;
            double Kdv = 0;
            double total = 0;
            double kdvRate = 0;
            DateTime invoiceDate = InvoiceDatePicker.SelectedDate.HasValue
                ? InvoiceDatePicker.SelectedDate.Value
                : DateTime.Now;

            if (!string.IsNullOrEmpty(AmountTextBox.Text))
            {
                if (!double.TryParse(AmountTextBox.Text, out amount))
                {
                    MessageBox.Show("Tutar değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Tutar boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(KdvTextBox.Text, out kdvRate))
            {
                MessageBox.Show("Kdv değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SaveKdvBox.IsChecked == true)
            {
                SaveKdvRate();
            }

            Kdv = amount * (kdvRate / 100);
            total = amount + Kdv;

            try
            {
                var invoice = new Invoice
                {
                    Musteri = currentCustomer.Name,
                    CustomerId = currentCustomer.Id,
                    FaturaNo = invoiceNo,
                    FaturaTarihi = invoiceDate,
                    Tutar = amount,
                    KDV = Kdv,
                    Toplam = total
                };
                int invoiceId = (int)_invoiceRepository.Add(invoice);

                var financial = new FinancialTransaction
                {
                    Aciklama = "Fatura " + invoiceNo,
                    Tutar = Kdv,
                    IslemTarihi = invoiceDate
                };

                int financialId = (int)_financialRepository.Add(financial);

                var transaction = new Transaction
                {
                    Aciklama = "Fatura Kesildi",
                    Notlar = invoiceNo,
                    Tutar = Kdv,
                    CustomerId = currentCustomer.Id,
                    FaturaId = invoiceId,
                    FinansalId = financialId
                };
                _transactionRepository.Add(transaction);

                MessageBox.Show("Fatura başarıyla eklendi!", "Hop!");

                UpdateCustomerDebt(Kdv);
                Back();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveKdvRate()
        {
            if (double.TryParse(KdvTextBox.Text, out double newKdv))
            {
                Settings.Default.Kdv = newKdv;
                Settings.Default.Save();
                MessageBox.Show("KDV oranı güncellendi.");
            }
            else
            {
                MessageBox.Show("Geçerli bir KDV oranı girin.");
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }

        private void Back()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            TransactionsPage transactionsPage = new TransactionsPage();
            transactionsPage.LoadCustomerTransactions(currentCustomer);
            mainWindow.MainFrame.Navigate(transactionsPage);
        }

        private void UpdateCustomerDebt(double amount)
        {
            _customerRepository.UpdateCustomerDebtById(amount, currentCustomer.Id);
        }
    }
}
