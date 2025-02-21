﻿using System;
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
    /// Interaction logic for InvoiceAddPage.xaml
    /// </summary>
    public partial class InvoiceAddPage : Page
    {
        private DatabaseHelper _dbHelper;
        private InvoiceRepository _invoiceRepository;
        private readonly TransactionRepository _transactionRepository;
        private readonly CustomerRepository _customerRepository;
        private Customer currentCustomer;
        public InvoiceAddPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _invoiceRepository = new InvoiceRepository(_dbHelper.GetConnection());
            _transactionRepository = new TransactionRepository(_dbHelper.GetConnection());
            _customerRepository = new CustomerRepository(_dbHelper.GetConnection());
            InvoiceDatePicker.SelectedDate = DateTime.Today;
        }

        public void GetCustomer(Customer customer)
        {
            currentCustomer = customer;
            TitleTextBlock.Text = $"{currentCustomer.Name} - Yeni Fatura Ekle";
        }

        private void SaveInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            string customer = CostumerTextBox.Text;
            string invoiceNo = InvoiceNoTextBox.Text;
            double amount = 0;
            double Kdv = 0;
            double total = 0;
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

            if (!string.IsNullOrEmpty(KdvTextBox.Text))
            {
                if (!double.TryParse(KdvTextBox.Text, out Kdv))
                {
                    MessageBox.Show("Kdv değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(TotalTextBox.Text))
            {
                if (!double.TryParse(TotalTextBox.Text, out total))
                {
                    MessageBox.Show("Toplam değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                var invoice = new Invoice
                {
                    Musteri = customer,
                    CustomerId = currentCustomer.Id,
                    FaturaNo = invoiceNo,
                    FaturaTarihi = invoiceDate.ToString("dd-MM-yyyy"),
                    Tutar = amount,
                    KDV = Kdv,
                    Toplam = total
                };
                int invoiceId = (int)_invoiceRepository.Add(invoice);

                var transaction = new Transaction
                {
                    Aciklama = "Fatura Kesildi",
                    Notlar = invoiceNo,
                    Tutar = Kdv,
                    CustomerId = currentCustomer.Id,
                    FaturaId = invoiceId
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
            double totalDebt = _customerRepository.GetCustomerDebtById(currentCustomer.Id);

            totalDebt += amount;

            Customer customer = _customerRepository.GetById(currentCustomer.Id);
            customer.Debt = totalDebt;
            _customerRepository.Update(customer);
        }
    }
}
