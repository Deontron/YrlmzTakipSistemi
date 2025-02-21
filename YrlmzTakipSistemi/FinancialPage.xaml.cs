﻿using System;
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
using YrlmzTakipSistemi.Repositories;

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
            List<MonthlySummary> summaries = _financialRepository.GetMonthlySummaries(year);

            LoadFinancialData(summaries);
        }

        public void LoadFinancialTransactions(string month, string year)
        {
            List<FinancialTransaction> transactions = _financialRepository.GetFinancialTransactions(month, year);

            LoadFinancialData(transactions);
        }

        public void LoadYearlySummaries()
        {
            List<YearlySummary> summaries = _financialRepository.GetYearlySummaries();

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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            printHelper.PrintDataGrid(FinancialDataGrid);
        }
    }
}
