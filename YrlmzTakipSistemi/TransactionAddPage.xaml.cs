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
using System.Data.Entity.Core.Common.CommandTrees;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for TransactionAddPage.xaml
    /// </summary>
    public partial class TransactionAddPage : Page
    {

        private DatabaseHelper dbHelper;
        Customer currentCustomer;

        public TransactionAddPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
        }

        public void GetCustomer(Customer customer)
        {
            currentCustomer = customer;
            TitleTextBlock.Text = $"{currentCustomer.Name} - İşlem Ekle";
        }

        private void SaveTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            string description = DescriptionTextBox.Text;
            string note = NoteTextBox.Text;

            double price = 0;
            int quantity = 0;
            double amount = 0;
            double paid = 0;

            if (!string.IsNullOrEmpty(PriceTextBox.Text))
            {
                if (double.TryParse(PriceTextBox.Text, out price))
                {
                }
                else
                {
                    MessageBox.Show("Fiyat değeri geçersiz");
                return;
                }
            }

            if (!string.IsNullOrEmpty(QuantityTextBox.Text))
            {
                if (int.TryParse(QuantityTextBox.Text, out quantity))
                {
                }
                else
                {
                    MessageBox.Show("Adet değeri geçersiz");
                return;
                }
            }

            if (!string.IsNullOrEmpty(AmountTextBox.Text))
            {
                if (double.TryParse(AmountTextBox.Text, out amount))
                {
                }
                else
                {
                    MessageBox.Show("Ücret değeri geçersiz");
                return;
                }
            }

            if (!string.IsNullOrEmpty(PaidTextBox.Text))
            {
                if (double.TryParse(PaidTextBox.Text, out paid))
                {
                }
                else
                {
                    MessageBox.Show("Ödenen değeri geçersiz");
                return;
                }
            }

            if (string.IsNullOrEmpty(description))
            {
                MessageBox.Show("İşlem Açıklaması boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"İşlem {description} başarıyla eklendi!", "Hop!");

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string insertQuery = "INSERT INTO Transactions (CustomerId, Aciklama, Notlar, Adet, BirimFiyat, Ucret, Odenen) VALUES (@CustomerId, @Description, @Note, @Quantity, @Price, @Amount, @Paid)";
                var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@Description", description);
                command.Parameters.AddWithValue("@Note", note);
                command.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.Parameters.AddWithValue("@Price", price);

                if(amount != 0)
                {
                    command.Parameters.AddWithValue("@Amount", amount);
                }
                else
                {
                    command.Parameters.AddWithValue("@Amount", (price * quantity));
                }

                command.Parameters.AddWithValue("@Paid", paid);

                command.ExecuteNonQuery();
            }

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
            PriceTextBox.Clear();
            QuantityTextBox.Clear();
            AmountTextBox.Clear();
            PaidTextBox.Clear();
        }
    }
}
