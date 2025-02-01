using Google.Apis.Drive.v3.Data;
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
    /// Interaction logic for PaymentAddPage.xaml
    /// </summary>
    public partial class PaymentAddPage : Page
    {
        private DatabaseHelper dbHelper;
        private Customer currentCustomer;
        public PaymentAddPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();

            TransactionDatePicker.SelectedDate = DateTime.Today;
        }

        public void GetCustomer(Customer customer)
        {
            currentCustomer = customer;
            TitleTextBlock.Text = $"{currentCustomer.Name} - Yeni Ödeme Ekle";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            TransactionsPage tp = new TransactionsPage();
            tp.LoadCustomerTransactions(currentCustomer);
            mainWindow.MainFrame.Navigate(tp);
        }

        private void SavePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            string description = DescriptionTextBox.Text;
            string note = NoteTextBox.Text;

            double amount = 0;
            string formattedDate = TransactionDatePicker.SelectedDate.HasValue
                ? TransactionDatePicker.SelectedDate.Value.ToString("dd-MM-yyyy")
                : string.Empty;

            if (!string.IsNullOrEmpty(AmountTextBox.Text))
            {
                if (double.TryParse(AmountTextBox.Text, out amount))
                {
                }
                else
                {
                    MessageBox.Show("Miktar değeri geçersiz");
                    return;
                }
            }

            if (string.IsNullOrEmpty(description))
            {
                MessageBox.Show("İşlem Açıklaması boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string insertQuery = "INSERT INTO Transactions (CustomerId, Aciklama, Notlar, Odenen) VALUES (@CustomerId, @Description, @Note, @Paid)";
                var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@Description", description);
                command.Parameters.AddWithValue("@Note", formattedDate);
                command.Parameters.AddWithValue("@Paid", amount);
                command.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);

                command.ExecuteNonQuery();
            }

            MessageBox.Show($"İşlem {description} başarıyla eklendi!", "Hop!");

            ClearForm();

            var mainWindow = (MainWindow)Application.Current.MainWindow;
            TransactionsPage transactionsPage = new TransactionsPage();
            transactionsPage.LoadCustomerTransactions(currentCustomer);
            mainWindow.MainFrame.Navigate(transactionsPage);
        }
        private void ClearForm()
        {
            DescriptionTextBox.Clear();
            NoteTextBox.Clear();
            AmountTextBox.Clear();
        }
    }
}
