using System;
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
        private ObservableCollection<Invoice> _invoices = new ObservableCollection<Invoice>();

        public InvoicePage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            printHelper = new PrintHelper();
            _invoiceRepository = new InvoiceRepository(_dbHelper.GetConnection());
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
                DateTime parsedDate;
                if (DateTime.TryParse(invoice.Tarih, out parsedDate))
                {
                    invoice.Tarih = parsedDate.ToString("dd.MM.yyyy");
                }
                if (DateTime.TryParse(invoice.FaturaTarihi, out parsedDate))
                {
                    invoice.FaturaTarihi = parsedDate.ToString("dd.MM.yyyy");
                }
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
                    _invoiceRepository.Delete(selectedInvoice.Id);

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
    }
}
