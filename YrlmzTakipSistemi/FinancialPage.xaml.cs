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
using System.Diagnostics;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for FinancialPage.xaml
    /// </summary>
    public partial class FinancialPage : Page
    {
        private DatabaseHelper dbHelper;

        public FinancialPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadYearlySummaries();
        }
        private void LoadFinancialData<T>(List<T> data)
        {
            FinancialDataGrid.ItemsSource = null;
            FinancialDataGrid.ItemsSource = data;
        }

        private void FinancialDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(FinancialDataGrid.SelectedItem == null)
            {
                return;
            }

            if (FinancialDataGrid.SelectedItem is YearlySummary selectedYear)
            {
                TitleTextBox.Text = "Aylık Gelir/Gider";
                LoadMonthlySummary(selectedYear.Yil);
            }
            else if (FinancialDataGrid.SelectedItem is MonthlySummary selectedMonth)
            {
                TitleTextBox.Text = "Aylık İşlemler";
                LoadFinancialTransactions(selectedMonth.Ay, selectedMonth.Yil);
            }
        }

        public void LoadMonthlySummary(string year)
        {
            List<MonthlySummary> summaries = new List<MonthlySummary>();

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                    strftime('%m', Tarih) AS Ay, 
                    SUM(CASE WHEN Tutar > 0 THEN Tutar ELSE 0 END) AS Gelir, 
                    SUM(CASE WHEN Tutar < 0 THEN ABS(Tutar) ELSE 0 END) AS Gider,
                    SUM(Tutar) AS Tutar
                    FROM FinancialTransactions 
                    WHERE strftime('%Y', Tarih) = @Year
                    GROUP BY Ay
                    ORDER BY Ay ASC;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Year", year);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            summaries.Add(new MonthlySummary
                            {
                                Ay = reader.GetString(0),
                                Gelir = reader.GetDouble(1),
                                Gider = reader.GetDouble(2),
                                Tutar = reader.GetDouble(3),
                                Yil = year 
                            });

                        }
                    }
                }
            }

            LoadFinancialData(summaries);
        }

        public void LoadFinancialTransactions(string month, string year)
        {
            List<FinancialTransaction> transactions = new List<FinancialTransaction>();

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT Id, strftime('%d-%m-%Y', Tarih) AS Tarih, Aciklama, Tutar
                    FROM FinancialTransactions
                    WHERE strftime('%Y', Tarih) = @Year
                    AND strftime('%m', Tarih) = @Month
                    ORDER BY Tarih ASC;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Year", year);
                    command.Parameters.AddWithValue("@Month", month);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(new FinancialTransaction
                            {
                                Id = reader.GetInt32(0),
                                Tarih = reader.GetString(1),
                                Aciklama = reader.GetString(2),
                                Tutar = reader.GetDouble(3)
                            });
                        }
                    }
                }
            }
            LoadFinancialData(transactions);
        }

        public void LoadYearlySummaries()
        {
            List<YearlySummary> summaries = new List<YearlySummary>();

            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                    strftime('%Y', Tarih) AS Yil, 
                    SUM(CASE WHEN Tutar > 0 THEN Tutar ELSE 0 END) AS Gelir, 
                    SUM(CASE WHEN Tutar < 0 THEN ABS(Tutar) ELSE 0 END) AS Gider,
                    SUM(Tutar) AS Tutar
                    FROM FinancialTransactions 
                    GROUP BY Yil 
                    ORDER BY Yil ASC;";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        summaries.Add(new YearlySummary
                        {
                            Yil = reader.GetString(0),
                            Gelir = reader.GetDouble(1),
                            Gider = reader.GetDouble(2),
                            Tutar = reader.GetDouble(3)
                        });
                    }
                }
            }

            LoadFinancialData(summaries);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new FinancialAddPage());
        }

        private void FinancialDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Id" || e.PropertyName == "CustomerId")
            {
                e.Column.Visibility = Visibility.Collapsed; 
            }
        }
    }
}
