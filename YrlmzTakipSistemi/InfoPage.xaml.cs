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

namespace YrlmzTakipSistemi
{
    /// <summary>
    /// Interaction logic for InfoPage.xaml
    /// </summary>
    public partial class InfoPage : Page
    {
        public InfoPage()
        {
            InitializeComponent();
        }

        public void LoadCustomerInfo(Customer customer)
        {
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
            mainWindow.MainFrame.Navigate(new CustomersPage());
        }

        private void UpdateCustomerButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
