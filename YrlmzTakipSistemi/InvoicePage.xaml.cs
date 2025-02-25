﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Google.Apis.Drive.v3.Data;
using YrlmzTakipSistemi.Repositories;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for InvoicePage.xaml
    /// </summary>
    public partial class InvoicePage : Page
    {
        private DatabaseHelper _dbHelper;
        private PrintHelper printHelper;
        private InvoiceRepository _invoiceRepository;
        private TransactionRepository _transactionRepository;
        private readonly CustomerRepository _customerRepository;
        private ObservableCollection<Invoice> _invoices = new ObservableCollection<Invoice>();
        private List<Invoice> filteredInvoices = new List<Invoice>();

        public InvoicePage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            printHelper = new PrintHelper();
            _invoiceRepository = new InvoiceRepository(_dbHelper.GetConnection());
            _transactionRepository = new TransactionRepository(_dbHelper.GetConnection());
            _customerRepository = new CustomerRepository(_dbHelper.GetConnection());
            LoadInvoices();
        }

        private void LoadInvoices()
        {
            var _invoices = GetInvoicesFromDatabase();
            InvoicesDataGrid.ItemsSource = _invoices;
            LoadTotalAmount();
        }

        private ObservableCollection<Invoice> GetInvoicesFromDatabase()
        {
            _invoices.Clear();
            var invoices = _invoiceRepository.GetAll();
            foreach (var invoice in invoices)
            {
                _invoices.Add(invoice);
            }
            return _invoices;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            var filteredInvoices = _invoices.Where(c => c.Musteri.ToLower().Contains(searchText)).ToList();

            InvoicesDataGrid.ItemsSource = filteredInvoices;
        }

        private void DeleteInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (InvoicesDataGrid.SelectedItem is Invoice selectedInvoice)
            {
                var result = MessageBox.Show(selectedInvoice.Musteri + " Silmek istediğinize emin misiniz?",
                                 "Silme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _customerRepository.UpdateCustomerDebtById(-selectedInvoice.KDV, selectedInvoice.CustomerId);
                    _invoiceRepository.Delete(selectedInvoice.Id);
                    _transactionRepository.DeleteWithDoc(selectedInvoice.Id, "FaturaId");

                    MessageBox.Show("Fatura başarıyla silindi.", "Hop!");
                    LoadInvoices();
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir fatura seçin.", "Hop!");
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (InvoicesDataGrid.SelectedItem is Invoice selectedInvoice)
            {
                var result = MessageBox.Show(selectedInvoice.Musteri + " Güncellemek istediğinize emin misiniz?",
                                 "Güncelleme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _invoiceRepository.Update(selectedInvoice);
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir fatura seçin.", "Hop!");
            }
        }

        private void LoadTotalAmount()
        {
            double total = _invoiceRepository.GetTotalAmount();
            SumTextBlock.Text = $"Toplam Tutar: {total.ToString("N2")} TL";
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            printHelper.PrintDataGrid(InvoicesDataGrid, "Faturalar");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_invoices == null || !_invoices.Any())
            {
                return;
            }

            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            var years = _invoices
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

            if (_invoices == null)
            {
                InvoicesDataGrid.ItemsSource = null;
                return;
            }

            filteredInvoices = _invoices
                .Where(t => t.Tarih.Year == selectedYear && (selectedMonth == 0 || t.Tarih.Month == selectedMonth))
                .ToList();

            InvoicesDataGrid.ItemsSource = filteredInvoices;
        }
    }
}
