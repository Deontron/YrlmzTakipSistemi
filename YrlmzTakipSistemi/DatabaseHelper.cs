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
        private string dbPath = "data source=company_tracking_system.db"; // Yeni veritabanı adı

        // Veritabanı bağlantısı oluşturma
        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(dbPath);
        }

        // Veritabanı dosyası var mı, kontrol et
        public void CheckDatabaseExists()
        {
            if (!File.Exists("company_tracking_system.db")) // Yeni veritabanı adı
            {
                SQLiteConnection.CreateFile("company_tracking_system.db"); // Yeni veritabanı adı
                CreateTables();
            }
        }

        // Veritabanı tablolarını oluşturma
        private void CreateTables()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                string createCustomersTable = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Email TEXT
                )";

                string createTransactionsTable = @"
                CREATE TABLE IF NOT EXISTS Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId INTEGER,
                    Amount REAL,
                    Date TEXT,
                    FOREIGN KEY(CustomerId) REFERENCES Customers(Id)
                )";

                var command = new SQLiteCommand(createCustomersTable, connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(createTransactionsTable, connection);
                command.ExecuteNonQuery();
            }
        }
    }
}
