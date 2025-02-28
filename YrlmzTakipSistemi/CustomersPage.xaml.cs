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
using YrlmzTakipSistemi.Repositories;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for CustomersPage.xaml
    /// </summary>
    public partial class CustomersPage : Page
    {
        private DatabaseHelper _dbHelper;
        private PrintHelper _printHelper;
        private CustomerRepository _customerRepository;
        private ObservableCollection<Customer> _customers = new ObservableCollection<Customer>();

        public CustomersPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _printHelper = new PrintHelper();
            _customerRepository = new CustomerRepository(_dbHelper.GetConnection());
            LoadCustomers();
            LoadTotalDebt();
        }

        private void LoadCustomers()
        {
            var customers = GetCustomersFromDatabase();
            CustomersDataGrid.ItemsSource = _customers;
        }

        private ObservableCollection<Customer> GetCustomersFromDatabase()
        {
            _customers.Clear();
            var customers = _customerRepository.GetAll();

            foreach (Customer customer in customers)
            {
                _customers.Add(customer);
            }
            return _customers;
        }

        private void CustomersDataGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem != null)
            {
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                TransactionsPage transactionsPage = new TransactionsPage();
                mainWindow.MainFrame.Navigate(transactionsPage);

                var selectedCustomer = (Customer)CustomersDataGrid.SelectedItem;
                transactionsPage.LoadCustomerTransactions(selectedCustomer);
            }
        }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new CustomerAddPage());
        }

        private void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem is Customer selectedCustomer)
            {
                var result = MessageBox.Show(selectedCustomer.Name + " Silmek istediğinize emin misiniz?",
                                 "Silme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _customerRepository.Delete(selectedCustomer.Id);

                    MessageBox.Show("Müşteri başarıyla silindi.", "Hop!");
                    LoadCustomers();
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir müşteri seçin.", "Hop!");
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            var filteredCustomers = _customers.Where(c => c.Name.ToLower().Contains(searchText)).ToList();

            CustomersDataGrid.ItemsSource = filteredCustomers;
        }

        private void LoadTotalDebt()
        {
            double totalDebt = _customerRepository.GetTotalDebt();

            SumTextBlock.Text = $"Toplam Alacak: {totalDebt:C}";
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem is Customer selectedCustomer)
            {
                var result = MessageBox.Show(selectedCustomer.Name + " Güncellemek istediğinize emin misiniz?",
                                 "Güncelleme Onayı",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _customerRepository.Update(selectedCustomer);
                    LoadTotalDebt();
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir müşteri seçin.", "Hop!");
            }

        }
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            _printHelper.PrintDataGrid(CustomersDataGrid, "Müşteriler");
        }

        private void CustomerInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem is Customer selectedCustomer)
            {
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                InfoPage infoPage = new InfoPage();
                infoPage.LoadCustomerInfo(selectedCustomer);
                mainWindow.MainFrame.Navigate(infoPage);
            }
            else
            {
                MessageBox.Show("Lütfen görüntülemek için bir müşteri seçin.", "Hop!");
            }
        }
    }
}
