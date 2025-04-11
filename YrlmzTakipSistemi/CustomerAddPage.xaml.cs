using System.Windows;
using System.Windows.Controls;
using YrlmzTakipSistemi.Repositories;

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for CustomerAddPage.xaml
    /// </summary>
    public partial class CustomerAddPage : Page
    {
        private DatabaseHelper _dbHelper;
        private CustomerRepository _customerRepository;
        public CustomerAddPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _customerRepository = new CustomerRepository(_dbHelper.GetConnection());
        }

        private void SaveCustomerButton_Click(object sender, RoutedEventArgs e)
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
                    MessageBox.Show("Miktar değeri geçersiz veya sıfırdan küçük olamaz.");
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
                Debt = amount
            };

            _customerRepository.Add(customer);

            MessageBox.Show($"Müşteri {name} başarıyla eklendi!", "Hop!");

            ClearForm();

            Back();
        }

        private void ClearForm()
        {
            NameTextBox.Clear();
            LongNameTextBox.Clear();
            ContactTextBox.Clear();
            AddressTextBox.Clear();
            TaxNoTextBox.Clear();
            TaxOfficeTextBox.Clear();
            SumTextBox.Clear();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }

        private void Back()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new CustomersPage());
        }
    }
}
