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
                    Tarih TEXT NOT NULL DEFAULT (strftime('%d-%m-%Y', 'now')),
                    Name TEXT NOT NULL,
                    Contact TEXT,
                    Debt REAL
                )";

                string createTransactionsTable = @"
                CREATE TABLE Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId INTEGER NOT NULL,
                    Tarih TEXT NOT NULL DEFAULT (strftime('%d-%m-%Y', 'now')),
                    Aciklama TEXT NOT NULL,
                    Notlar TEXT, 
                    Adet INTEGER,
                    BirimFiyat REAL,
                    Tutar REAL,
                    Odenen REAL,
                    AlacakDurumu REAL,
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
                );
                
                CREATE TRIGGER CalculateAmounts
                AFTER INSERT ON Transactions
                FOR EACH ROW
                BEGIN
                    UPDATE Transactions
                    SET 
                        Tutar = COALESCE(NEW.Tutar, NEW.BirimFiyat * NEW.Adet)
                    WHERE Id = NEW.Id;
                
                    UPDATE Transactions
                    SET 
                        AlacakDurumu = (
                    SELECT IFNULL(Tutar, 0) - IFNULL(NEW.Odenen, 0) 
                    FROM Transactions 
                    WHERE Id = NEW.Id
        )
                    WHERE Id = NEW.Id;
                END;
                
                CREATE TRIGGER UpdateAmounts
                AFTER UPDATE OF BirimFiyat, Adet, Tutar, Odenen ON Transactions
                FOR EACH ROW
                BEGIN
                    UPDATE Transactions
                    SET 
                        Tutar = COALESCE(NEW.Tutar, NEW.BirimFiyat * NEW.Adet)
                    WHERE Id = NEW.Id;
                
                    UPDATE Transactions
                    SET 
                        AlacakDurumu = (
                    SELECT IFNULL(Tutar, 0) - IFNULL(NEW.Odenen, 0) 
                    FROM Transactions 
                    WHERE Id = NEW.Id
        )
                    WHERE Id = NEW.Id;
                END;
                ";

                string createProductsTable = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId INTEGER NOT NULL,
                    Tarih TEXT NOT NULL DEFAULT (strftime('%d-%m-%Y', 'now')),
                    Isim TEXT NOT NULL,
                    Fiyat REAL,
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
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
            }
        }
    }
}
