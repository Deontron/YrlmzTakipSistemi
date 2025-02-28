using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseHelper dbHelper;

        public MainWindow()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            dbHelper.CheckDatabaseExists();
            MainFrame.Navigate(new CustomersPage());
            this.Closing += MainWindow_Closing;
        }

        private void CustomersButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CustomersPage());
        }

        private void IncomeExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new FinancialPage());
        }

        private void InvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new InvoicePage());
        }

        private void CheckBillButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PaymentsPage());
        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            GoogleDriveBackup.UploadBackup();
        }

        private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Yedeklensin ve çıkılsın mı?",
                "Onay",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question,
                MessageBoxResult.Cancel
            );

            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await BackupToGoogleDrive();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Yedekleme sırasında hata oluştu:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                }
            }
        }

        private async Task BackupToGoogleDrive()
        {
            GoogleDriveBackup.UploadBackup();
            await Task.Delay(1000);
        }
    }
}