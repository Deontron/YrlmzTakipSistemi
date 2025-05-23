﻿using System.Windows;
using System.Windows.Controls;
using YrlmzTakipSistemi.Repositories;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for PaymentAddPage.xaml
    /// </summary>
    public partial class PaymentAddPage : Page
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly TransactionRepository _transactionRepository;
        private readonly CustomerRepository _customerRepository;
        private DatabaseHelper _dbHelper;
        private Customer currentCustomer;
        public PaymentAddPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _paymentRepository = new PaymentRepository(_dbHelper.GetConnection());
            _transactionRepository = new TransactionRepository(_dbHelper.GetConnection());
            _customerRepository = new CustomerRepository(_dbHelper.GetConnection());

            PaymentDatePicker.SelectedDate = DateTime.Today;
        }

        public void GetCustomer(Customer customer)
        {
            currentCustomer = customer;
            TitleTextBlock.Text = $"{currentCustomer.Name} - Yeni Ödeme Ekle";
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

        private void SavePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            string description = DescriptionTextBox.Text;
            string debtor = DebtorTextBox.Text;
            string place = PlaceTextBox.Text;
            double amount = 0;
            int category = GetSelectedPaymentMethod();
            int paidState = GetPaidState();
            string paidDescription = GetPaidDescription();
            DateTime formattedDate = PaymentDatePicker.SelectedDate.HasValue
                ? PaymentDatePicker.SelectedDate.Value.Date : DateTime.Now.Date;

            if (!string.IsNullOrEmpty(AmountTextBox.Text))
            {
                if (!double.TryParse(AmountTextBox.Text, out amount))
                {
                    MessageBox.Show("Miktar değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Miktar boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (category == 0)
            {
                MessageBox.Show("Kategori boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (paidState == 0)
            {
                MessageBox.Show("Ödeme durumu boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int? paymentId = null; 

                if (category != 3)
                {
                    var payment = new Payment
                    {
                        Musteri = currentCustomer.Name,
                        Borclu = debtor,
                        KasideYeri = place,
                        Kategori = category,
                        Tutar = amount,
                        OdemeTarihi = formattedDate,
                        OdemeDurumu = paidState,
                        OdemeDescription = paidDescription,
                        CustomerId = currentCustomer.Id,
                        Tarih = DateTime.Today
                    };
                    paymentId = (int?)_paymentRepository.Add(payment);
                }

                var transaction = new Transaction
                {
                    CustomerId = currentCustomer.Id,
                    Notlar = formattedDate.ToString("dd-MM-yyyy"),
                    Odenen = amount,
                    Tarih = DateTime.Today
                };

                if(category == 1)
                {
                    transaction.Aciklama = "Çek Alındı";
                }
                else if (category == 2)
                {
                    transaction.Aciklama = "Senet Alındı";
                }
                else if (category == 3)
                {
                    transaction.Aciklama = "Ödeme Alındı";
                }

                if (paymentId.HasValue)
                {
                    transaction.OdemeId = paymentId.Value;
                }
                _transactionRepository.Add(transaction);

                UpdateCustomerDebt(amount);

                MessageBox.Show($"{currentCustomer.Name} ödemesi başarıyla eklendi!", "Hop!");

                ClearForm();
                Back();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetSelectedPaymentMethod()
        {
            if (CheckButton.IsChecked == true)
            {
                return 1;
            }
            else if (BillButton.IsChecked == true)
            {
                return 2;
            }
            else if (CashButton.IsChecked == true)
            {
                return 3;
            }
            else
            {
                return 0;
            }
        }

        private string GetPaidDescription()
        {
            if (UnpaidButton.IsChecked == true)
            {
                return "Ödenmedi";
            }
            else if (CollectButton.IsChecked == true)
            {
                return "Tahsil";
            }
            else if (BankButton.IsChecked == true)
            {
                return "Bankada";
            }
            else if (OtherButton.IsChecked == true)
            {
                return OtherTextBox.Text;
            }
            else
            {
                return "Bilinmiyor";
            }
        }

        private int GetPaidState()
        {
            if (UnpaidButton.IsChecked == true)
            {
                return 1;
            }
            else if (BankButton.IsChecked == true)
            {
                return 2;
            }
            else if (CollectButton.IsChecked == true)
            {
                return 3;
            }
            else if (OtherButton.IsChecked == true)
            {
                return 4;
            }
            else
            {
                return 0;
            }
        }

        private void ClearForm()
        {
            DescriptionTextBox.Clear();
            DebtorTextBox.Clear();
            AmountTextBox.Clear();
            PlaceTextBox.Clear();
        }

        private void UpdateCustomerDebt(double amount)
        {
            _customerRepository.UpdateCustomerDebtById(-amount, currentCustomer.Id);
        }
    }
}
