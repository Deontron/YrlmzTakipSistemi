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
        }

        private void SaveTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string phone = PhoneTextBox.Text;
            string email = EmailTextBox.Text;
            string address = AddressTextBox.Text;


            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("İşlem adı boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"İşlem {name} başarıyla eklendi!", "Hop!");

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string insertQuery = "INSERT INTO Transactions (CustomerId, ProductName, Date, Quantity, Price) VALUES (@CustomerId, @Name, @Email, @Quantity, @Price)";
                var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@CustomerId", currentCustomer.Id);
                command.Parameters.AddWithValue("@Quantity", currentCustomer.Id);
                command.Parameters.AddWithValue("@Price", currentCustomer.Id);

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
            NameTextBox.Clear();
            PhoneTextBox.Clear();
            EmailTextBox.Clear();
            AddressTextBox.Clear();
        }
    }
}
