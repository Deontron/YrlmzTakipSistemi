using System.Windows;
using System.Windows.Controls;
using YrlmzTakipSistemi.Repositories;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for FinancialAddPage.xaml
    /// </summary>
    public partial class FinancialAddPage : Page
    {
        private DatabaseHelper _dbHelper;
        private FinancialRepository _financialRepository;

        public FinancialAddPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _financialRepository = new FinancialRepository(_dbHelper.GetConnection());
            FinancialDatePicker.SelectedDate = DateTime.Today;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Back();
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
            var financialTransaction = new FinancialTransaction
            {
                Aciklama = description,
                IslemTarihi = formattedDate,
                Tutar = income - expance
            };

            _financialRepository.Add(financialTransaction);
            MessageBox.Show($"{description} başarıyla eklendi!", "Hop!");

            Back();
        }

        private void Back()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new FinancialPage());
        }
    }
}
