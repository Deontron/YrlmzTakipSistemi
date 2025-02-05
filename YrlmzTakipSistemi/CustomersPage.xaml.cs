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
using System.Collections.ObjectModel;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for CustomersPage.xaml
    /// </summary>
    public partial class CustomersPage : Page
    {
        private DatabaseHelper dbHelper;
        private ObservableCollection<Customer> customers = new ObservableCollection<Customer>();

        public CustomersPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadCustomers();
            LoadTotalDebt();
        }

        private void LoadCustomers()
        {
            var customers = GetCustomersFromDatabase();
            CustomersDataGrid.ItemsSource = customers;
        }

        private ObservableCollection<Customer> GetCustomersFromDatabase()
        {
            customers.Clear();

            using (var connection = dbHelper.GetConnection())
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
                            Tarih = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Name = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Contact = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Debt = reader.IsDBNull(4) ? 0 : reader.GetDouble(4)
                        });
                    }
                }
                connection.Close();

            }

            return customers;
        }

        private void CustomersDataGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem != null)
            {
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                TransactionsPage transactionsPage = new TransactionsPage();
                mainWindow.MainFrame.Navigate(transactionsPage);

                var selectedCustomer = (Customer)CustomersDataGrid.SelectedItem;
                transactionsPage.LoadCustomerTransactions(selectedCustomer);
            }
            else
            {
                MessageBox.Show("Lütfen bir müşteri seçin.", "Hop!");
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

                var result = MessageBox.Show(selectedCustomer.Name + " Silmek istediğinize emin misiniz?",
                                 "Silme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
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
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir müşteri seçin.", "Hop!");
            }
        }

        private bool DeleteCustomerFromDatabase(int customerId)
        {
            using (var connection = dbHelper.GetConnection())
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
                            connection.Close();

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

        private void CustomersDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var customer = e.Row.Item as Customer;
            if (customer != null)
            {
                bool updateSuccess = UpdateCustomerInDatabase(customer);

                if (updateSuccess)
                {
                    LoadCustomers();
                }
                else
                {
                    MessageBox.Show("Müşteri güncellenemedi!");
                }
            }
        }

        private bool UpdateCustomerInDatabase(Customer customer)
        {
            using (var connection = dbHelper.GetConnection())
            {
                try
                {
                    connection.Open();
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        MessageBox.Show("Veritabanı bağlantısı başarısız!");
                        return false;
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string updateCustomerQuery = "UPDATE Customers SET Name = @Name, Contact = @Contact, Debt = @Debt WHERE Id = @Id";
                            using (var updateCustomerCommand = new SQLiteCommand(updateCustomerQuery, connection, transaction))
                            {
                                updateCustomerCommand.Parameters.AddWithValue("@Name", customer.Name);
                                updateCustomerCommand.Parameters.AddWithValue("@Contact", customer.Contact);
                                updateCustomerCommand.Parameters.AddWithValue("@Debt", customer.Debt);
                                updateCustomerCommand.Parameters.AddWithValue("@Id", customer.Id);

                                int rowsAffected = updateCustomerCommand.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    MessageBox.Show("Güncellenen müşteri bulunamadı. Id: " + customer.Id);
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            transaction.Commit();
                            MessageBox.Show("Müşteri başarıyla güncellendi.");
                            LoadTotalDebt();
                            connection.Close();

                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Güncelleme hatası: " + ex.Message);
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bağlantı hatası: " + ex.Message);
                    return false;
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            var filteredCustomers = customers.Where(c => c.Name.ToLower().Contains(searchText)).ToList();

            CustomersDataGrid.ItemsSource = filteredCustomers;
        }

        private void LoadTotalDebt()
        {
            double totalDebt = 0;

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string query = "SELECT SUM(Debt) FROM Customers";

                SQLiteCommand command = new SQLiteCommand(query, connection);

                var result = command.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    totalDebt = Convert.ToDouble(result);
                }
                connection.Close();

            }

            SumTextBlock.Text = $"Toplam Alacak: {totalDebt.ToString("N2")} TL";
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem != null)
            {
                var selectedCustomer = (Customer)CustomersDataGrid.SelectedItem;

                var result = MessageBox.Show(selectedCustomer.Name + " Güncellemek istediğinize emin misiniz?",
                                 "Güncelleme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    UpdateCustomerInDatabase(selectedCustomer);
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir müşteri seçin.", "Hop!");
            }
        }
    }
}
