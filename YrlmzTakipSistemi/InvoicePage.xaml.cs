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
using Google.Apis.Drive.v3.Data;

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
            LoadInvoices();
        }

        private void LoadInvoices()
        {
            var invoices = GetInvoicesFromDatabase();
            InvoicesDataGrid.ItemsSource = invoices;
            LoadTotalAmount();
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
                connection.Close();

            }

            return Invoices;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            var filteredInvoices = Invoices.Where(c => c.Musteri.ToLower().Contains(searchText)).ToList();

            InvoicesDataGrid.ItemsSource = filteredInvoices;
        }

        private void DeleteInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (InvoicesDataGrid.SelectedItem != null)
            {
                var selectedInvoice = (Invoice)InvoicesDataGrid.SelectedItem;

                var result = MessageBox.Show(selectedInvoice.Musteri + " Silmek istediğinize emin misiniz?",
                                 "Silme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = DeleteInvoiceFromDatabase(selectedInvoice.Id);

                    if (success)
                    {
                        MessageBox.Show("Fatura başarıyla silindi.", "Hop!");
                        LoadInvoices();
                    }
                    else
                    {
                        MessageBox.Show("Silme işlemi başarısız oldu.", "Hop!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir fatura seçin.", "Hop!");
            }
        }

        private bool DeleteInvoiceFromDatabase(int invoiceId)
        {
            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string deleteInvoiceQuery = "DELETE FROM Invoices WHERE Id = @Id";
                        using (var deleteInvoiceCommand = new SQLiteCommand(deleteInvoiceQuery, connection, transaction))
                        {
                            deleteInvoiceCommand.Parameters.AddWithValue("@Id", invoiceId);
                            int rowsAffected = deleteInvoiceCommand.ExecuteNonQuery();

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

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (InvoicesDataGrid.SelectedItem != null)
            {
                var selectedInvoice= (Invoice)InvoicesDataGrid.SelectedItem;

                var result = MessageBox.Show(selectedInvoice.Musteri + " Güncellemek istediğinize emin misiniz?",
                                 "Güncelleme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    UpdateInvoiceInDatabase(selectedInvoice);
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir fatura seçin.", "Hop!");
            }
        }

        private bool UpdateInvoiceInDatabase(Invoice invoice)
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
                            string updateInvoiceQuery = "UPDATE Invoices SET Musteri = @Musteri, FaturaNo = @FaturaNo, FaturaTarihi = @FaturaTarihi, Tutar = @Tutar, KDV = @KDV, Toplam = @Toplam WHERE Id = @Id";
                            using (var updateInvoiceCommand = new SQLiteCommand(updateInvoiceQuery, connection, transaction))
                            {
                                updateInvoiceCommand.Parameters.AddWithValue("@Musteri", invoice.Musteri);
                                updateInvoiceCommand.Parameters.AddWithValue("@FaturaNo", invoice.FaturaNo);
                                updateInvoiceCommand.Parameters.AddWithValue("@FaturaTarihi", invoice.FaturaTarihi);
                                updateInvoiceCommand.Parameters.AddWithValue("@Tutar", invoice.Tutar);
                                updateInvoiceCommand.Parameters.AddWithValue("@KDV", invoice.KDV);
                                updateInvoiceCommand.Parameters.AddWithValue("@Toplam", invoice.Toplam);
                                updateInvoiceCommand.Parameters.AddWithValue("@Id", invoice.Id);

                                int rowsAffected = updateInvoiceCommand.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    MessageBox.Show("Güncellenen fatura bulunamadı. Id: " + invoice.Id);
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            transaction.Commit();
                            MessageBox.Show("Fatura başarıyla güncellendi.");
                            LoadTotalAmount();
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

        private void LoadTotalAmount()
        {
            double total = 0;

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string query = "SELECT SUM(Toplam) FROM Invoices";

                SQLiteCommand command = new SQLiteCommand(query, connection);

                var result = command.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    total = Convert.ToDouble(result);
                }
                connection.Close();

            }

            SumTextBlock.Text = $"Toplam Tutar: {total.ToString("N2")} TL";
        }
    }
}
