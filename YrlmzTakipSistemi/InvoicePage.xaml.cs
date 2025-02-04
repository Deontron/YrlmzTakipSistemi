using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for InvoicePage.xaml
    /// </summary>
    public partial class InvoicePage : Page
    {
        private DatabaseHelper dbHelper;
        private ObservableCollection<Invoice> Invoices = new ObservableCollection<Invoice>();

        public InvoicePage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
        }

        private void LoadCustomers()
        {
            var invoices = GetInvoicesFromDatabase();
            InvoicesDataGrid.ItemsSource = invoices;
        }

        private ObservableCollection<Invoice> GetInvoicesFromDatabase()
        {
            Invoices.Clear();

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Invoices";
                SQLiteCommand command = new SQLiteCommand(query, connection);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Invoices.Add(new Invoice
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1),
                            Tarih = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Musteri = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            FaturaNo = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            FaturaTarihi = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            Tutar = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
                            KDV = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),
                            Toplam = reader.IsDBNull(8) ? 0 : reader.GetDouble(8)
                        });
                    }
                }
            }

            return Invoices;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void DeleteInvoiceButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new InvoiceAddPage());
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
