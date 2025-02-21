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
using YrlmzTakipSistemi.Repositories;


namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for PaymentsPage.xaml
    /// </summary>
    public partial class PaymentsPage : Page
    {
        private readonly PaymentRepository _paymentRepository;
        private DatabaseHelper _dbHelper;
        private PrintHelper printHelper;
        private ObservableCollection<Payment> _payments = new ObservableCollection<Payment>();

        public PaymentsPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            printHelper = new PrintHelper();
            _paymentRepository = new PaymentRepository(_dbHelper.GetConnection());
            LoadPayments();
        }

        public void LoadPayments()
        {
            _payments.Clear();

            var payments = _paymentRepository.GetAll();
            foreach (var payment in payments)
            {
                DateTime parsedDate;
                if (DateTime.TryParse(payment.Tarih, out parsedDate))
                {
                    payment.Tarih = parsedDate.ToString("dd.MM.yyyy");
                }
                if (DateTime.TryParse(payment.OdemeTarihi, out parsedDate))
                {
                    payment.OdemeTarihi = parsedDate.ToString("dd.MM.yyyy");
                }
                _payments.Add(payment);
            }

            foreach (var payment in _payments)
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

            PaymentsDataGrid.ItemsSource = _payments;
            UpdateTotalAmount();
        }

        private void DeletePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsDataGrid.SelectedItem is Payment selectedPayment)
            {
                var result = MessageBox.Show(selectedPayment.Musteri + " Silmek istediğinize emin misiniz?",
                                 "Silme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _paymentRepository.Delete(selectedPayment.Id);

                    MessageBox.Show("Ödeme başarıyla silindi.", "Hop!");
                    LoadPayments();
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir ödeme seçin.", "Hop!");
            }
        }

        private void PaymentsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PaymentsDataGrid.SelectedItem is Payment selectedPayment)
            {
                _paymentRepository.ChangePaidState(selectedPayment);

                LoadPayments();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            var filteredCustomers = _payments.Where(c => c.Musteri.ToLower().Contains(searchText)).ToList();

            PaymentsDataGrid.ItemsSource = filteredCustomers;
        }

        private void UpdateTotalAmount()
        {
            double totalAmount = _paymentRepository.GetTotalAmount();

            SumTextBlock.Text = $"Toplam Alacak: {totalAmount.ToString("N2")} TL";
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsDataGrid.SelectedItem is Payment selectedPayment)
            {
                var result = MessageBox.Show(selectedPayment.Musteri + " Güncellemek istediğinize emin misiniz?",
                                 "Güncelleme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _paymentRepository.Update(selectedPayment);
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir müşteri seçin.", "Hop!");
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            printHelper.PrintDataGrid(PaymentsDataGrid);
        }
    }
}
