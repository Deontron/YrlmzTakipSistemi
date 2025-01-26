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
    /// Interaction logic for CustomersPage.xaml
    /// </summary>
    public partial class CustomersPage : Page
    {
        private DatabaseHelper _databaseHelper;

        public CustomersPage()
        {
            InitializeComponent();
            _databaseHelper = new DatabaseHelper();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            var customers = GetCustomersFromDatabase();
            CustomersDataGrid.ItemsSource = customers;
        }

        private List<Customer> GetCustomersFromDatabase()
        {
            List<Customer> customers = new List<Customer>();

            using (var connection = _databaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Customers";
                SQLiteCommand command = new SQLiteCommand(query, connection);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(new Customer
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Email = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }

            return customers;
        }


        private void ShowTransactionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem != null)
            {
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                TransactionsPage transactionsPage = new TransactionsPage();
                mainWindow.MainFrame.Navigate(transactionsPage);
            
                var selectedCustomer = (Customer)CustomersDataGrid.SelectedItem;
                transactionsPage.LoadCustomerTransactions(selectedCustomer.Id);
            }
            else
            {
                MessageBox.Show("Lütfen bir müşteri seçin.","Hop!");
            }
        }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new CustomerAddPage());
        }

        private void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem != null)
            {
                var selectedCustomer = (Customer)CustomersDataGrid.SelectedItem;

                bool success = DeleteCustomerFromDatabase(selectedCustomer.Id);

                if (success)
                {
                    MessageBox.Show("Müşteri başarıyla silindi.", "Hop!");
                    LoadCustomers();  
                }
                else
                {
                    MessageBox.Show("Silme işlemi başarısız oldu.", "Hop!");
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir müşteri seçin.", "Hop!");
            }
        }

        private bool DeleteCustomerFromDatabase(int customerId)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string deleteTransactionsQuery = "DELETE FROM Transactions WHERE CustomerId = @CustomerId";
                        using (var deleteTransactionsCommand = new SQLiteCommand(deleteTransactionsQuery, connection, transaction))
                        {
                            deleteTransactionsCommand.Parameters.AddWithValue("@CustomerId", customerId);
                            deleteTransactionsCommand.ExecuteNonQuery();
                        }

                        string deleteCustomerQuery = "DELETE FROM Customers WHERE Id = @Id";
                        using (var deleteCustomerCommand = new SQLiteCommand(deleteCustomerQuery, connection, transaction))
                        {
                            deleteCustomerCommand.Parameters.AddWithValue("@Id", customerId);
                            int rowsAffected = deleteCustomerCommand.ExecuteNonQuery();

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

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class Transaction
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Date { get; set; }
        public String ProductName { get; set; }
        public float Quantity { get; set; }
        public double Price { get; set; }
    }
}
