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
                            Tarih = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),  
                            Aciklama = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),  
                            Notlar = reader.IsDBNull(4) ? null : reader.GetString(4),  
                            Adet = reader.GetInt32(5),
                            BirimFiyat = reader.IsDBNull(6) ? 0.0 : reader.GetDouble(6),
                            Ucret = reader.IsDBNull(7) ? 0.0 : reader.GetDouble(7),
                            Odenen = reader.IsDBNull(8) ? 0.0 : reader.GetDouble(8),
                            AlacakDurumu = reader.IsDBNull(9) ? 0.0 : reader.GetDouble(9)
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

                var result = MessageBox.Show(selectedTransaction.Aciklama + " Silmek istediğinize emin misiniz?",
                                 "Silme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = DeleteTransactionFromDatabase(selectedTransaction.Id);

                    if (success)
                    {
                        MessageBox.Show("İşlem başarıyla silindi.", "Hop!");
                        LoadCustomerTransactions(currentCustomer);
                    }
                    else
                    {
                        MessageBox.Show("Silme işlemi başarısız oldu.", "Hop!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir işlem seçin.", "Hop!");
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
