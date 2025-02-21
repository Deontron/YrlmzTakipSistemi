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
using YrlmzTakipSistemi.Repositories;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for InfoPage.xaml
    /// </summary>
    public partial class InfoPage : Page
    {
        private DatabaseHelper _dbHelper;
        private readonly CustomerRepository _customerRepository;

        Customer currentCustomer;
        public InfoPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _customerRepository = new CustomerRepository(_dbHelper.GetConnection());
        }

        public void LoadCustomerInfo(Customer customer)
        {
            currentCustomer = customer;

            NameTextBox.Text = customer.Name;
            LongNameTextBox.Text = customer.LongName;
            ContactTextBox.Text = customer.Contact;
            AddressTextBox.Text = customer.Address;
            TaxNoTextBox.Text = customer.TaxNo;
            TaxOfficeTextBox.Text = customer.TaxOffice;
            SumTextBox.Text = customer.Debt.ToString("N2");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }

        private void Back()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            TransactionsPage transactionsPage = new TransactionsPage();
            transactionsPage.LoadCustomerTransactions(currentCustomer);
            mainWindow.MainFrame.Navigate(transactionsPage);
        }

        private void UpdateCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(NameTextBox.Text + " Güncellemek istediğinize emin misiniz?",
                             "Güncelleme Onayı",
                             MessageBoxButton.YesNo,
                             MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                UpdateCustomerInDatabase();
            }
        }

        private void UpdateCustomerInDatabase()
        {
            string name = NameTextBox.Text;
            string longName = LongNameTextBox.Text;
            string contact = ContactTextBox.Text;
            string address = AddressTextBox.Text;
            string taxNo = TaxNoTextBox.Text;
            string taxOffice = TaxOfficeTextBox.Text;
            double amount = 0;

            if (!string.IsNullOrEmpty(SumTextBox.Text))
            {
                if (!double.TryParse(SumTextBox.Text, out amount))
                {
                    MessageBox.Show("Miktar değeri geçersiz.");
                    return;
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Müşteri adı boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var customer = new Customer
            {
                Name = name,
                LongName = longName,
                Contact = contact,
                Address = address,
                TaxNo = taxNo,
                TaxOffice = taxOffice,
                Debt = amount,
                Id = currentCustomer.Id
            };
            _customerRepository.Update(customer);

            SetCurrentCustomer();
            Back();
        }

        private void SetCurrentCustomer()
        {
            currentCustomer = _customerRepository.GetById(currentCustomer.Id);
        }
    }
}
