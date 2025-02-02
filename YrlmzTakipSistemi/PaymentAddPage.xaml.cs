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

            PaymentDatePicker.SelectedDate = DateTime.Today;
        }

        public void GetCustomer(Customer customer)
        {
            currentCustomer = customer;
            TitleTextBlock.Text = $"{currentCustomer.Name} - Yeni Ödeme Ekle";
            CostumerTextBox.Text = currentCustomer.Name;
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
            string customerName = CostumerTextBox.Text;
            string debtor = DebtorTextBox.Text;
            string place = PlaceTextBox.Text;
            double amount = 0;
            int category = GetSelectedPaymentMethod();
            bool paidState = GetPaidState();
            string formattedDate = PaymentDatePicker.SelectedDate.HasValue
                ? PaymentDatePicker.SelectedDate.Value.ToString("dd-MM-yyyy")
                : string.Empty;

            if (!string.IsNullOrEmpty(AmountTextBox.Text))
            {
                if (!double.TryParse(AmountTextBox.Text, out amount))
                {
                    MessageBox.Show("Miktar değeri geçersiz.");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Miktar boş olamaz.");
                return;
            }

            if (string.IsNullOrEmpty(customerName))
            {
                MessageBox.Show("Müşteri ismi boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();

                    string insertTransactionQuery = "INSERT INTO Transactions (CustomerId, Aciklama, Notlar, Odenen) VALUES (@CustomerId, @Description, @Note, @Paid)";
                    var transactionCommand = new SQLiteCommand(insertTransactionQuery, connection);
                    transactionCommand.Parameters.AddWithValue("@Description", customerName);
                    transactionCommand.Parameters.AddWithValue("@Note", formattedDate);
                    transactionCommand.Parameters.AddWithValue("@Paid", amount);
                    transactionCommand.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                    transactionCommand.ExecuteNonQuery();

                    string insertPaymentQuery = "INSERT INTO Payments (CustomerId, Musteri, Borclu, KasideYeri, Kategori, Tutar, OdemeTarihi, OdendiMi) VALUES (@CustomerId, @Musteri, @Borclu, @KasideYeri, @Kategori, @Tutar, @OdemeTarihi, @OdendiMi)";
                    var paymentCommand = new SQLiteCommand(insertPaymentQuery, connection);
                    paymentCommand.Parameters.AddWithValue("@Musteri", customerName);
                    paymentCommand.Parameters.AddWithValue("@Borclu", debtor);
                    paymentCommand.Parameters.AddWithValue("@KasideYeri", place);
                    paymentCommand.Parameters.AddWithValue("@Kategori", category);
                    paymentCommand.Parameters.AddWithValue("@Tutar", amount);
                    paymentCommand.Parameters.AddWithValue("@OdemeTarihi", formattedDate);
                    paymentCommand.Parameters.AddWithValue("@OdendiMi", paidState);
                    paymentCommand.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                    paymentCommand.ExecuteNonQuery();
                }

                MessageBox.Show($"{customerName} ödemesi başarıyla eklendi!", "Hop!");

                ClearForm();
                Back();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private bool GetPaidState()
        {
            if (PaidBox.IsChecked == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ClearForm()
        {
            CostumerTextBox.Clear();
            CostumerTextBox.Clear();
            AmountTextBox.Clear();
        }
    }
}
