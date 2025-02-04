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
            string formattedDate = PaymentDatePicker.SelectedDate.HasValue
                ? PaymentDatePicker.SelectedDate.Value.ToString("dd-MM-yyyy")
                : string.Empty;

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
                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();

                    string insertTransactionQuery = "INSERT INTO Transactions (CustomerId, Aciklama, Notlar, Odenen) VALUES (@CustomerId, @Description, @Note, @Paid)";
                    var transactionCommand = new SQLiteCommand(insertTransactionQuery, connection);
                    transactionCommand.Parameters.AddWithValue("@Description", description);
                    transactionCommand.Parameters.AddWithValue("@Note", formattedDate);
                    transactionCommand.Parameters.AddWithValue("@Paid", amount);
                    transactionCommand.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                    transactionCommand.ExecuteNonQuery();

                    if (category != 3)
                    {
                        string insertPaymentQuery = "INSERT INTO Payments (CustomerId, Musteri, Borclu, KasideYeri, Kategori, Tutar, OdemeTarihi, OdemeDurumu, OdemeDescription) VALUES (@CustomerId, @Musteri, @Borclu, @KasideYeri, @Kategori, @Tutar, @OdemeTarihi, @OdemeDurumu, @OdemeDescription)";
                        var paymentCommand = new SQLiteCommand(insertPaymentQuery, connection);
                        paymentCommand.Parameters.AddWithValue("@Musteri", currentCustomer.Name);
                        paymentCommand.Parameters.AddWithValue("@Borclu", debtor);
                        paymentCommand.Parameters.AddWithValue("@KasideYeri", place);
                        paymentCommand.Parameters.AddWithValue("@Kategori", category);
                        paymentCommand.Parameters.AddWithValue("@Tutar", amount);
                        paymentCommand.Parameters.AddWithValue("@OdemeTarihi", formattedDate);
                        paymentCommand.Parameters.AddWithValue("@OdemeDurumu", paidState);
                        paymentCommand.Parameters.AddWithValue("@OdemeDescription", paidDescription);
                        paymentCommand.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                        paymentCommand.ExecuteNonQuery();
                    }
                }

                UpdateCustomerDebt(amount);

                MessageBox.Show($"{currentCustomer.Name} ödemesi başarıyla eklendi!", "Hop!");

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
            else if (CollectButton.IsChecked == true)
            {
                return 2;
            }
            else if (BankButton.IsChecked == true)
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
            }

            totalDebt -= amount;

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string updateQuery = "UPDATE Customers SET Debt = @Debt WHERE Id = @CustomerId";
                SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@Debt", totalDebt);
                updateCommand.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                updateCommand.ExecuteNonQuery();
            }
        }
    }
}
