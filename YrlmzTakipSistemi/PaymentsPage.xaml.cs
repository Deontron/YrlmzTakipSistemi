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
                            OdendiMi = reader.IsDBNull(9) ? false : reader.GetBoolean(9)
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
                        case 3:
                            payment.KategoriDescription = "Nakit";
                            break;
                        default:
                            payment.KategoriDescription = "Bilinmeyen";
                            break;
                    }

                    payment.OdendiMiDescription = payment.OdendiMi ? "Ödendi" : "Ödenmedi";
                }

                PaymentsDataGrid.ItemsSource = payments;
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
                selectedPayment.OdendiMi = !selectedPayment.OdendiMi;

                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();
                    string updateQuery = "UPDATE Payments SET OdendiMi = @YeniDurum WHERE Id = @PaymentId";
                    SQLiteCommand command = new SQLiteCommand(updateQuery, connection);
                    command.Parameters.AddWithValue("@YeniDurum", selectedPayment.OdendiMi);
                    command.Parameters.AddWithValue("@PaymentId", selectedPayment.Id);
                    command.ExecuteNonQuery();
                }

                LoadPayments();
            }
        }


    }
}
