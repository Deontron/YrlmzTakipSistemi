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
    /// Interaction logic for TransactionsPage.xaml
    /// </summary>
    public partial class TransactionsPage : Page
    {
        private DatabaseHelper _databaseHelper;
        private Customer currentCustomer;

        public TransactionsPage()
        {
            InitializeComponent();
            _databaseHelper = new DatabaseHelper();
        }

        public void LoadCustomerTransactions(Customer customer)
        {
            currentCustomer = customer;

            TitleTextBlock.Text = $"{customer.Name} - İşlemler"; 
            using (var connection = _databaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Transactions WHERE CustomerId = @CustomerId";
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerId", customer.Id);

                List<Transaction> transactions = new List<Transaction>();

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1),
                            Date = reader.GetString(2),
                            ProductName = reader.GetString(3),
                            Quantity = reader.GetInt32(4),
                            Price = reader.GetDouble(5)
                        });
                    }
                }

                TransactionsDataGrid.ItemsSource = transactions;
            }
        }

        private void AddTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            TransactionAddPage transactionAddPage = new TransactionAddPage();
            transactionAddPage.GetCustomer(currentCustomer);
            mainWindow.MainFrame.Navigate(transactionAddPage);
        }

        private void DeleteTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem != null)
            {
                var selectedTransaction = (Transaction)TransactionsDataGrid.SelectedItem;

                bool success = DeleteTransactionFromDatabase(selectedTransaction.Id);

                if (success)
                {
                    MessageBox.Show("İşlem başarıyla silindi.");
                    LoadCustomerTransactions(currentCustomer);
                }
                else
                {
                    MessageBox.Show("Silme işlemi başarısız oldu.");
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir İşlem seçin.");
            }
        }

        private bool DeleteTransactionFromDatabase(int transactionId)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string deleteTransactionsQuery = "DELETE FROM Transactions WHERE Id = @Id";
                        using (var deleteTransactionsCommand = new SQLiteCommand(deleteTransactionsQuery, connection, transaction))
                        {
                            deleteTransactionsCommand.Parameters.AddWithValue("@Id", transactionId);

                            int rowsAffected = deleteTransactionsCommand.ExecuteNonQuery();

                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}
