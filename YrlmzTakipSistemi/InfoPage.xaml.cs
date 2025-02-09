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
    /// Interaction logic for InfoPage.xaml
    /// </summary>
    public partial class InfoPage : Page
    {
        private DatabaseHelper dbHelper;

        Customer currentCustomer;
        public InfoPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
        }

        public void LoadCustomerInfo(Customer customer)
        {
            currentCustomer = customer;

            NameTextBox.Text = customer.Name;
            LongNameTextBox.Text = customer.LongName;
            ContactTextBox.Text = customer.Contact;
            AddressTextBox.Text = customer.Address;
            TaxNoTextBox.Text = customer.TaxNo;
            TaxOfficeTextBox.Text = customer.TaxOffice;
            SumTextBox.Text = customer.Debt.ToString("N2");
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

        private void UpdateCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(NameTextBox.Text + " Güncellemek istediğinize emin misiniz?",
                             "Güncelleme Onayı",
                             MessageBoxButton.YesNo,
                             MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                UpdateCustomerInDatabase();
            }
        }

        private bool UpdateCustomerInDatabase()
        {
            string name = NameTextBox.Text;
            string longName = LongNameTextBox.Text;
            string contact = ContactTextBox.Text;
            string address = AddressTextBox.Text;
            string taxNo = TaxNoTextBox.Text;
            string taxOffice = TaxOfficeTextBox.Text;
            double amount = 0;

            if (!string.IsNullOrEmpty(SumTextBox.Text))
            {
                if (!double.TryParse(SumTextBox.Text, out amount))
                {
                    MessageBox.Show("Miktar değeri geçersiz veya sıfırdan küçük olamaz.");
                    return false;
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Müşteri adı boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

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
                            string updateCustomerQuery = "UPDATE Customers SET Name = @Name, LongName = @LongName, Contact = @Contact, Address = @Address, TaxNo = @TaxNo, TaxOffice=@TaxOffice, Debt = @Debt WHERE Id = @Id";
                            using (var updateCustomerCommand = new SQLiteCommand(updateCustomerQuery, connection, transaction))
                            {
                                updateCustomerCommand.Parameters.AddWithValue("@Name", name);
                                updateCustomerCommand.Parameters.AddWithValue("@LongName", longName);
                                updateCustomerCommand.Parameters.AddWithValue("@Contact", contact);
                                updateCustomerCommand.Parameters.AddWithValue("@Address", address);
                                updateCustomerCommand.Parameters.AddWithValue("@TaxNo", taxNo);
                                updateCustomerCommand.Parameters.AddWithValue("@TaxOffice", taxOffice);
                                updateCustomerCommand.Parameters.AddWithValue("@Debt", amount);
                                updateCustomerCommand.Parameters.AddWithValue("@Id", currentCustomer.Id);

                                int rowsAffected = updateCustomerCommand.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    MessageBox.Show("Güncellenen müşteri bulunamadı. Id: " + currentCustomer.Id);
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            transaction.Commit();
                            MessageBox.Show("Müşteri başarıyla güncellendi.");
                            connection.Close();
                            SetCurrentCustomer();
                            Back();

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

        private void SetCurrentCustomer()
        {
            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Customers WHERE Id = @Id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", currentCustomer.Id);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentCustomer = new Customer
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                LongName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                Contact = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                Address = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                TaxNo = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                TaxOffice = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                Debt = reader.IsDBNull(7) ? 0 : reader.GetDouble(7)
                            };
                        }
                    }
                }
            }
        }
    }
}
