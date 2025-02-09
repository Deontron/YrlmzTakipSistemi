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
        private string dbPath = "data source=company_tracking_system.db";

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(dbPath);
        }

        public void CheckDatabaseExists()
        {
            if (!File.Exists("company_tracking_system.db"))
            {
                SQLiteConnection.CreateFile("company_tracking_system.db");
                CreateTables();
            }
        }

        private void CreateTables()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                string createCustomersTable = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    LongName TEXT,
                    Contact TEXT,
                    Address TEXT,
                    TaxNo TEXT,
                    TaxOffice TEXT,
                    Debt REAL
                )";

                string createProductsTable = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId INTEGER NOT NULL,
                    Tarih TEXT NOT NULL DEFAULT (strftime('%d-%m-%Y', 'now')),
                    Isim TEXT NOT NULL,
                    Fiyat REAL,
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
                )";

                //DocType TEXT CHECK(DocType IN('Invoice', 'Payment') OR DocType IS NULL), 

                string createTransactionsTable = @"
                CREATE TABLE Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId INTEGER NOT NULL,
                    DocId INTEGER,  
                    Tarih TEXT NOT NULL DEFAULT (strftime('%d-%m-%Y', 'now')),
                    Aciklama TEXT NOT NULL,
                    Notlar TEXT, 
                    Adet INTEGER,
                    BirimFiyat REAL,
                    Tutar REAL DEFAULT 0,
                    Odenen REAL DEFAULT 0,
                    AlacakDurumu REAL GENERATED ALWAYS AS (Tutar - Odenen) STORED,
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
                )";

                string createPaymentsTable = @"
                CREATE TABLE IF NOT EXISTS Payments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                    CustomerId INTEGER NOT NULL,
                    Tarih TEXT NOT NULL DEFAULT (strftime('%d-%m-%Y', 'now')), 
                    Musteri TEXT NOT NULL,  
                    Borclu TEXT,  
                    KasideYeri TEXT,  
                    Kategori INTEGER NOT NULL, 
                    Tutar REAL NOT NULL, 
                    OdemeTarihi TEXT NOT NULL, 
                    OdemeDurumu INTEGER NOT NULL, 
                    OdemeDescription TEXT NOT NULL, 
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
                )";

                string createInvoicesTable = @"
                CREATE TABLE IF NOT EXISTS Invoices (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId INTEGER NOT NULL,
                    Tarih TEXT NOT NULL DEFAULT (strftime('%d-%m-%Y', 'now')),
                    Musteri TEXT NOT NULL,
                    FaturaNo TEXT NOT NULL,
                    FaturaTarihi TEXT,
                    Tutar REAL,
                    KDV REAL,
                    Toplam REAL,
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)  
                )";

                var command = new SQLiteCommand(createCustomersTable, connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(createTransactionsTable, connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(createProductsTable, connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(createPaymentsTable, connection);
                command.ExecuteNonQuery();
                
                command = new SQLiteCommand(createInvoicesTable, connection);
                command.ExecuteNonQuery();
            }
        }
    }
}
