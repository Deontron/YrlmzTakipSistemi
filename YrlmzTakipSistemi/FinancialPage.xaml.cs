using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using YrlmzTakipSistemi.Repositories;
using System.Globalization;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for FinancialPage.xaml
    /// </summary>
    public partial class FinancialPage : Page
    {
        private DatabaseHelper _dbHelper;
        private PrintHelper printHelper;
        private FinancialRepository _financialRepository;
        private int pageId = 0;
        private string currentYear;
        private string currentMonth;
        public FinancialPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            printHelper = new PrintHelper();
            _financialRepository = new FinancialRepository(_dbHelper.GetConnection());
            LoadYearlySummaries();
        }
        private void LoadFinancialData<T>(List<T> data)
        {
            FinancialDataGrid.ItemsSource = null;
            FinancialDataGrid.ItemsSource = data;
        }

        private void FinancialDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FinancialDataGrid.SelectedItem == null)
            {
                return;
            }

            if (FinancialDataGrid.SelectedItem is YearlySummary selectedYear)
            {
                currentYear = selectedYear.Yil;
                pageId = 1;
                TitleTextBox.Text = "Aylık Gelir/Gider";
                LoadMonthlySummary(selectedYear.Yil);
            }
            else if (FinancialDataGrid.SelectedItem is MonthlySummary selectedMonth)
            {
                pageId = 2;
                currentMonth = selectedMonth.Ay;
                TitleTextBox.Text = "Aylık İşlemler";
                LoadFinancialTransactions(selectedMonth.Ay, selectedMonth.Yil);
            }
        }

        private void Back()
        {
            if (pageId == 0)
            {
                return;
            }

            if (pageId == 1)
            {
                pageId = 0;
                TitleTextBox.Text = "Yıllık Gelir/Gider";
                LoadYearlySummaries();
            }
            else if (pageId == 2)
            {
                pageId = 1;
                TitleTextBox.Text = "Aylık Gelir/Gider";
                LoadMonthlySummary(currentYear);
            }
        }

        public void LoadMonthlySummary(string year)
        {
            var (summaries, total) = _financialRepository.GetMonthlySummaries(year);

            LoadFinancialData(summaries);
            DisplayTotal(total); 
        }

        public void LoadFinancialTransactions(string month, string year)
        {
            var (transactions, total) = _financialRepository.GetFinancialTransactions(month, year);

            LoadFinancialData(transactions);
            DisplayTotal(total);
        }

        public void LoadYearlySummaries()
        {
            var (summaries, total) = _financialRepository.GetYearlySummaries();

            LoadFinancialData(summaries);
            DisplayTotal(total);
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

            if (e.PropertyType == typeof(DateTime))
            {
                var column = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding(e.PropertyName) { StringFormat = "dd-MM-yyyy" }
                };
                e.Column = column;
            }

            if (e.PropertyType == typeof(double) || e.PropertyType == typeof(decimal))
            {
                var column = e.Column as DataGridTextColumn;
                if (column != null)
                {
                    column.Binding = new Binding(e.PropertyName)
                    {
                        StringFormat = "C2",
                        ConverterCulture = new CultureInfo("tr-TR")
                    };
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (FinancialDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Lütfen silmek için bir kayıt seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (pageId == 0)
            {
                if (FinancialDataGrid.SelectedItem is YearlySummary selectedYear)
                {
                    var result = MessageBox.Show($"{selectedYear.Yil} yılının tüm verilerini silmek istediğinize emin misiniz?",
                                                 "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        _financialRepository.DeleteYearlyData(selectedYear.Yil);
                        MessageBox.Show($"{selectedYear.Yil} yılına ait tüm veriler silindi.", "Bilgi");
                        LoadYearlySummaries();
                    }
                }
            }
            else if (pageId == 1)
            {
                if (FinancialDataGrid.SelectedItem is MonthlySummary selectedMonth)
                {
                    var result = MessageBox.Show($"{selectedMonth.Ay}/{selectedMonth.Yil} tarihindeki tüm işlemleri silmek istediğinize emin misiniz?",
                                                 "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _financialRepository.DeleteMonthlyData(selectedMonth.Ay, selectedMonth.Yil);
                        MessageBox.Show($"{selectedMonth.Ay}/{selectedMonth.Yil} tarihindeki tüm işlemler silindi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadMonthlySummary(currentYear);
                    }
                }
            }
            else if (pageId == 2)
            {
                if (FinancialDataGrid.SelectedItem is FinancialTransaction selectedTransaction)
                {
                    var result = MessageBox.Show("Bu işlemi silmek istediğinize emin misiniz?",
                                                 "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _financialRepository.Delete(selectedTransaction.Id);
                        MessageBox.Show("İşlem başarıyla silindi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadFinancialTransactions(currentMonth, currentYear);
                    }
                }
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            printHelper.PrintDataGrid(FinancialDataGrid, "Gelir/Gider");
        }

        private void DisplayTotal(double total)
        {
            SumTextBlock.Text = $"Toplam: {total:C}"; 
        }
    }
}
