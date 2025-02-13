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
    /// Interaction logic for FinancialAddPage.xaml
    /// </summary>
    public partial class FinancialAddPage : Page
    {
        private DatabaseHelper dbHelper;
        public FinancialAddPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            FinancialDatePicker.SelectedDate = DateTime.Today;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveFinancialButton_Click(object sender, RoutedEventArgs e)
        {
            string description = DescriptionTextBox.Text;
            double income = 0;
            double expance = 0;
            DateTime formattedDate = FinancialDatePicker.SelectedDate.HasValue
                ? FinancialDatePicker.SelectedDate.Value.Date : DateTime.Now.Date;

            if (!string.IsNullOrEmpty(IncomeTextBox.Text))
            {
                if (!double.TryParse(IncomeTextBox.Text, out income))
                {
                    MessageBox.Show("Gelir değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(ExpanceTextBox.Text))
            {
                if (!double.TryParse(ExpanceTextBox.Text, out expance))
                {
                    MessageBox.Show("Gider değeri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();

                    string insertTransactionQuery = "INSERT INTO FinancialTransactions (Tarih, Aciklama, Tutar) VALUES (@Date, @Description, @Amount)";
                    var transactionCommand = new SQLiteCommand(insertTransactionQuery, connection);
                    transactionCommand.Parameters.AddWithValue("@Description", description);
                    transactionCommand.Parameters.AddWithValue("@Date", formattedDate);
                    transactionCommand.Parameters.AddWithValue("@Amount", income-expance);
                    transactionCommand.ExecuteNonQuery();

                    connection.Close();
                }

                MessageBox.Show($"{description} başarıyla eklendi!", "Hop!");

                Back();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back()
        {

        }
    }
}
