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
                    Email TEXT
                )";

                string createTransactionsTable = @"
                CREATE TABLE Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId INTEGER NOT NULL,
                    Tarih TEXT NOT NULL DEFAULT CURRENT_DATE,
                    Aciklama TEXT NOT NULL,
                    Notlar TEXT, 
                    Adet INTEGER,
                    BirimFiyat REAL,
                    Ucret REAL,
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
                        Ucret = COALESCE(NEW.Ucret, NEW.BirimFiyat * NEW.Adet)
                    WHERE Id = NEW.Id;
                
                    UPDATE Transactions
                    SET 
                        AlacakDurumu = (
            SELECT Ucret - IFNULL(NEW.Odenen, 0) 
            FROM Transactions 
            WHERE Id = NEW.Id
        )
                    WHERE Id = NEW.Id;
                END;
                
                CREATE TRIGGER UpdateAmounts
                AFTER UPDATE OF BirimFiyat, Adet, Ucret, Odenen ON Transactions
                FOR EACH ROW
                BEGIN
                    UPDATE Transactions
                    SET 
                        Ucret = COALESCE(NEW.Ucret, NEW.BirimFiyat * NEW.Adet)
                    WHERE Id = NEW.Id;
                
                    UPDATE Transactions
                    SET 
                        AlacakDurumu = (
            SELECT Ucret - IFNULL(NEW.Odenen, 0) 
            FROM Transactions 
            WHERE Id = NEW.Id
        )
                    WHERE Id = NEW.Id;
                END;
                ";


                var command = new SQLiteCommand(createCustomersTable, connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(createTransactionsTable, connection);
                command.ExecuteNonQuery();
            }
        }
    }
}
