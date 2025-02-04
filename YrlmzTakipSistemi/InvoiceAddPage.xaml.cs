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

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for InvoiceAddPage.xaml
    /// </summary>
    public partial class InvoiceAddPage : Page
    {
        private DatabaseHelper dbHelper;
        public InvoiceAddPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();

            InvoiceDatePicker.SelectedDate = DateTime.Today;
        }

        private void SaveInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            string customer = CostumerTextBox.Text;
            string invoiceNo = InvoiceNoTextBox.Text;
            double amount = 0;
            double Kdv = 0;
            double total = 0;
            string invoiceDate = InvoiceDatePicker.SelectedDate.HasValue
                ? InvoiceDatePicker.SelectedDate.Value.ToString("dd-MM-yyyy")
                : string.Empty;

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
                if (!double.TryParse(AmountTextBox.Text, out amount))
                {
                    MessageBox.Show("Kdv değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(TotalTextBox.Text))
            {
                if (!double.TryParse(AmountTextBox.Text, out amount))
                {
                    MessageBox.Show("Toplam değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();

                        string insertInvoiceQuery = "INSERT INTO Invoices (CostumerId, Musteri, FaturaNo, FaturaTarihi, Tutar, KDV, Toplam) VALUES (@CostumerId, @Musteri, @FaturaNo, @FaturaTarihi, @Tutar, @KDV, @Toplam)";
                        var invoiceCommand = new SQLiteCommand(insertInvoiceQuery, connection);
                        invoiceCommand.Parameters.AddWithValue("@Musteri", customer);
                        invoiceCommand.Parameters.AddWithValue("@CostumerId", 0);
                        invoiceCommand.Parameters.AddWithValue("@FaturaNo", invoiceNo);
                        invoiceCommand.Parameters.AddWithValue("@FaturaTarihi", invoiceDate);
                        invoiceCommand.Parameters.AddWithValue("@Tutar", amount);
                        invoiceCommand.Parameters.AddWithValue("@KDV", Kdv);
                        invoiceCommand.Parameters.AddWithValue("@Toplam", total);
                        invoiceCommand.ExecuteNonQuery();
                }

                MessageBox.Show("Fatura başarıyla eklendi!", "Hop!");

                Back();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new InvoicePage());
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }
    }
}
