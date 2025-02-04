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
using Google.Apis.Drive.v3.Data;
using System.Numerics;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for PaymentsPage.xaml
    /// </summary>
    public partial class PaymentsPage : Page
    {
        private DatabaseHelper dbHelper;
        private ObservableCollection<Payment> payments = new ObservableCollection<Payment>();

        public PaymentsPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadPayments();
        }

        public void LoadPayments()
        {
            payments.Clear();

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string query = "SELECT * FROM Payments";
                SQLiteCommand command = new SQLiteCommand(query, connection);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        payments.Add(new Payment
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1),
                            Tarih = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Musteri = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Borclu = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            KasideYeri = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            Kategori = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                            Tutar = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),
                            OdemeTarihi = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                            OdemeDurumu = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                            OdemeDescription = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
                        });
                    }
                }

                foreach (var payment in payments)
                {
                    switch (payment.Kategori)
                    {
                        case 1:
                            payment.KategoriDescription = "Çek";
                            break;
                        case 2:
                            payment.KategoriDescription = "Senet";
                            break;
                        default:
                            payment.KategoriDescription = "Bilinmeyen";
                            break;
                    }
                }

                PaymentsDataGrid.ItemsSource = payments;
                UpdateTotalAmount();
            }
        }


        private void DeletePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsDataGrid.SelectedItem != null)
            {
                var selectedPayment = (Payment)PaymentsDataGrid.SelectedItem;

                var result = MessageBox.Show(selectedPayment.Musteri + " Silmek istediğinize emin misiniz?",
                                 "Silme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = DeletePaymentFromDatabase(selectedPayment.Id);

                    if (success)
                    {
                        MessageBox.Show("Ödeme başarıyla silindi.", "Hop!");
                        LoadPayments();
                    }
                    else
                    {
                        MessageBox.Show("Silme işlemi başarısız oldu.", "Hop!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir ödeme seçin.", "Hop!");
            }
        }

        private bool DeletePaymentFromDatabase(int paymentId)
        {
            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string deletePaymentsQuery = "DELETE FROM Payments WHERE Id = @Id";
                        using (var deletePaymentsCommand = new SQLiteCommand(deletePaymentsQuery, connection, transaction))
                        {
                            deletePaymentsCommand.Parameters.AddWithValue("@Id", paymentId);

                            int rowsAffected = deletePaymentsCommand.ExecuteNonQuery();

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

        private void PaymentsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PaymentsDataGrid.SelectedItem is Payment selectedPayment)
            {
                //selectedPayment.OdendiMi = !selectedPayment.OdendiMi;

                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();
                    string updateQuery = "UPDATE Payments SET OdemeDurumu = @YeniDurum WHERE Id = @PaymentId";
                    SQLiteCommand command = new SQLiteCommand(updateQuery, connection);
                    //command.Parameters.AddWithValue("@YeniDurum", selectedPayment.OdendiMi);
                    command.Parameters.AddWithValue("@PaymentId", selectedPayment.Id);
                    command.ExecuteNonQuery();
                }

                LoadPayments();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            var filteredCustomers = payments.Where(c => c.Musteri.ToLower().Contains(searchText)).ToList();

            PaymentsDataGrid.ItemsSource = filteredCustomers;
        }

        private void UpdateTotalAmount()
        {
            double totalAmount = 0;

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();

                string query = "SELECT Tutar FROM Payments WHERE OdemeDurumu = 1 OR OdemeDurumu = 3";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0)) 
                        {
                            totalAmount += reader.GetDouble(0);
                        }
                    }
                }
            }

            SumTextBlock.Text = $"Toplam Alacak: {totalAmount.ToString("N2")} TL";

        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsDataGrid.SelectedItem != null)
            {
                var selectedPayment = (Payment)PaymentsDataGrid.SelectedItem;

                var result = MessageBox.Show(selectedPayment.Musteri + " Güncellemek istediğinize emin misiniz?",
                                 "Güncelleme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    UpdatePaymentInDatabase(selectedPayment);
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir müşteri seçin.", "Hop!");
            }
        }

        private bool UpdatePaymentInDatabase(Payment payment)
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
                            string updatePaymentQuery = @"
                        UPDATE Payments 
                        SET Musteri = @Musteri, 
                            Borclu = @Borclu, 
                            KasideYeri = @KasideYeri, 
                            Kategori = @Kategori, 
                            Tutar = @Tutar, 
                            OdemeTarihi = @OdemeTarihi, 
                            OdemeDurumu = @OdemeDurumu, 
                            OdemeDescription = @OdemeDescription 
                        WHERE Id = @Id";

                            using (var updatePaymentCommand = new SQLiteCommand(updatePaymentQuery, connection, transaction))
                            {
                                updatePaymentCommand.Parameters.AddWithValue("@Musteri", payment.Musteri);
                                updatePaymentCommand.Parameters.AddWithValue("@Borclu", payment.Borclu);
                                updatePaymentCommand.Parameters.AddWithValue("@KasideYeri", payment.KasideYeri);
                                updatePaymentCommand.Parameters.AddWithValue("@Kategori", payment.Kategori);
                                updatePaymentCommand.Parameters.AddWithValue("@Tutar", payment.Tutar);
                                updatePaymentCommand.Parameters.AddWithValue("@OdemeTarihi", payment.OdemeTarihi);
                                updatePaymentCommand.Parameters.AddWithValue("@OdemeDurumu", payment.OdemeDurumu);
                                updatePaymentCommand.Parameters.AddWithValue("@OdemeDescription", payment.OdemeDescription);
                                updatePaymentCommand.Parameters.AddWithValue("@Id", payment.Id);

                                int rowsAffected = updatePaymentCommand.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    MessageBox.Show("Güncellenen ödeme bulunamadı. Id: " + payment.Id);
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            transaction.Commit(); 

                            MessageBox.Show("Ödeme başarıyla güncellendi.");

                            LoadPayments();
                            UpdateTotalAmount();

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

    }
}
