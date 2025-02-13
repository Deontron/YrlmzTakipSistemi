using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace YrlmzTakipSistemi
{
    internal class DatabaseHelper
    {
        private const string DbFileName = "company_tracking_system.db";
        private const string DbPath = "data source=" + DbFileName;

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(DbPath);
        }

        public void CheckDatabaseExists()
        {
            bool dbExists = File.Exists(DbFileName);

            if (!dbExists)
            {
                SQLiteConnection.CreateFile(DbFileName);
            }

            using (var connection = GetConnection())
            {
                connection.Open();

                using (var command = new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
                {
                    command.ExecuteNonQuery();
                }

                if (!dbExists)
                {
                    CreateTables(connection);
                }
                connection.Close();
            }
        }

        private void CreateTables(SQLiteConnection connection)
        {
            string createCustomersTable = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    LongName TEXT,
                    Contact TEXT,
                    Address TEXT,
                    TaxNo TEXT,
                    TaxOffice TEXT,
                    Debt DECIMAL(10,2) DEFAULT 0
                )";

            string createProductsTable = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId INTEGER NOT NULL,
                    Tarih DATETIME NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                    Isim TEXT NOT NULL,
                    Fiyat DECIMAL(10,2),
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
                )";

            string createTransactionsTable = @"
                CREATE TABLE IF NOT EXISTS Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId INTEGER NOT NULL,
                    DocId INTEGER,  
                    Tarih DATETIME NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                    Aciklama TEXT NOT NULL,
                    Notlar TEXT, 
                    Adet INTEGER,
                    BirimFiyat DECIMAL(10,2),
                    Tutar DECIMAL(10,2) DEFAULT 0,
                    Odenen DECIMAL(10,2) DEFAULT 0,
                    AlacakDurumu DECIMAL(10,2) GENERATED ALWAYS AS (Tutar - Odenen) STORED,
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
                )";

            string createPaymentsTable = @"
                CREATE TABLE IF NOT EXISTS Payments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                    CustomerId INTEGER NOT NULL,
                    Tarih DATETIME NOT NULL DEFAULT (CURRENT_TIMESTAMP), 
                    Musteri TEXT NOT NULL,  
                    Borclu TEXT,  
                    KasideYeri TEXT,  
                    Kategori INTEGER NOT NULL, 
                    Tutar DECIMAL(10,2) NOT NULL, 
                    OdemeTarihi DATETIME NOT NULL, 
                    OdemeDurumu INTEGER NOT NULL, 
                    OdemeDescription TEXT NOT NULL, 
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
                )";

            string createInvoicesTable = @"
                CREATE TABLE IF NOT EXISTS Invoices (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId INTEGER NOT NULL,
                    Tarih DATETIME NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                    Musteri TEXT NOT NULL,
                    FaturaNo TEXT NOT NULL,
                    FaturaTarihi DATETIME,
                    Tutar DECIMAL(10,2),
                    KDV DECIMAL(10,2),
                    Toplam DECIMAL(10,2),
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)  
                )";

            string createFinancialTransactionsTable = @"
                CREATE TABLE IF NOT EXISTS FinancialTransactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Tarih DATETIME NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                    Aciklama TEXT NOT NULL,
                    Tutar DECIMAL(10,2) NOT NULL
                )";

            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = createCustomersTable;
                command.ExecuteNonQuery();

                command.CommandText = createProductsTable;
                command.ExecuteNonQuery();

                command.CommandText = createTransactionsTable;
                command.ExecuteNonQuery();

                command.CommandText = createPaymentsTable;
                command.ExecuteNonQuery();

                command.CommandText = createInvoicesTable;
                command.ExecuteNonQuery();

                command.CommandText = createFinancialTransactionsTable;
                command.ExecuteNonQuery();
            }
        }
    }
}
