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
        }

        private void CustomersButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CustomersPage());
        }

        private void IncomeExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new IncomeExpensePage());
        }

        private void InvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new InvoicePage());
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Uygulamadan çıkmak istediğinize emin misiniz?",
                                 "Çıkış Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}