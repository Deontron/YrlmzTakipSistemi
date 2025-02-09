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
        private Customer currentCustomer;
        public InvoiceAddPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();

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
                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();

                    string insertTransactionQuery = "INSERT INTO Transactions (CustomerId, Aciklama, Notlar, Tutar) VALUES (@CustomerId, @Description, @Note, @Amount)";
                    var transactionCommand = new SQLiteCommand(insertTransactionQuery, connection);
                    transactionCommand.Parameters.AddWithValue("@Description", "Fatura Kesildi");
                    transactionCommand.Parameters.AddWithValue("@Note", invoiceNo);
                    transactionCommand.Parameters.AddWithValue("@Amount", Kdv);
                    transactionCommand.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                    transactionCommand.ExecuteNonQuery();

                    string insertInvoiceQuery = "INSERT INTO Invoices (CustomerId, Musteri, FaturaNo, FaturaTarihi, Tutar, KDV, Toplam) VALUES (@CostumerId, @Musteri, @FaturaNo, @FaturaTarihi, @Tutar, @KDV, @Toplam)";
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
            double totalDebt = 0;

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string query = "SELECT Debt FROM Customers WHERE Id = @CustomerId";
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);

                var result = command.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    totalDebt = Convert.ToDouble(result);
                }

                connection.Close();
            }

            totalDebt += amount;

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string updateQuery = "UPDATE Customers SET Debt = @Debt WHERE Id = @CustomerId";
                SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@Debt", totalDebt);
                updateCommand.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                updateCommand.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}
