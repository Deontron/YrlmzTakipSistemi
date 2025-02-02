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
    /// Interaction logic for CustomerAddPage.xaml
    /// </summary>
    public partial class CustomerAddPage : Page
    {
        private DatabaseHelper dbHelper;
        public CustomerAddPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
        }

        private void SaveCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string contact = ContactTextBox.Text;
            double amount = 0;

            if (!string.IsNullOrEmpty(SumTextBox.Text))
            {
                if (!double.TryParse(SumTextBox.Text, out amount))
                {
                    MessageBox.Show("Miktar değeri geçersiz veya sıfırdan küçük olamaz.");
                    return;
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Müşteri adı boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"Müşteri {name} başarıyla eklendi!", "Hop!");

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string insertQuery = "INSERT INTO Customers (Name, Contact, Debt) VALUES (@Name, @Contact, @Debt)";
                var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Contact", contact);
                command.Parameters.AddWithValue("@Debt", amount);

                command.ExecuteNonQuery();
            }

            ClearForm();

            Back();
        }

        private void ClearForm()
        {
            NameTextBox.Clear();
            ContactTextBox.Clear();
            SumTextBox.Clear();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }

        private void Back()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new CustomersPage());
        }
    }
}
