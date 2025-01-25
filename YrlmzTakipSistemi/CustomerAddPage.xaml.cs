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

        // Müşteri Kaydetme Butonu Tıklama Olayı
        private void SaveCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string phone = PhoneTextBox.Text;
            string email = EmailTextBox.Text;
            string address = AddressTextBox.Text;

            // Burada veritabanına kaydetme işlemi yapılabilir.
            MessageBox.Show($"Müşteri {name} başarıyla eklendi!");

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Müşteri adı boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string insertQuery = "INSERT INTO Customers (Name, Email) VALUES (@Name, @Email)";
                var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Email", email);

                command.ExecuteNonQuery();
            }

            ClearForm();
        }

        // Formu Temizleme
        private void ClearForm()
        {
            NameTextBox.Clear();
            PhoneTextBox.Clear();
            EmailTextBox.Clear();
            AddressTextBox.Clear();
        }
    }
}
