using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace YrlmzTakipSistemi.Repositories
{
    class FinancialRepository : Repository<FinancialTransaction>
    {
        public FinancialRepository(SQLiteConnection connection) : base(connection, "FinancialTransactions") { }

        public (List<YearlySummary> summaries, double total) GetYearlySummaries()
        {
            List<YearlySummary> summaries = new List<YearlySummary>();
            double totalAmount = 0;

            _connection.Open();
            string query = @"
        SELECT 
        strftime('%Y', IslemTarihi) AS Yil, 
        SUM(CASE WHEN Tutar > 0 THEN Tutar ELSE 0 END) AS Gelir, 
        SUM(CASE WHEN Tutar < 0 THEN ABS(Tutar) ELSE 0 END) AS Gider,
        SUM(Tutar) AS Tutar
        FROM FinancialTransactions 
        GROUP BY Yil 
        ORDER BY Yil ASC;";

            using (var command = new SQLiteCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    double yearTotal = reader.IsDBNull(3) ? 0 : reader.GetDouble(3);
                    summaries.Add(new YearlySummary
                    {
                        Yil = reader.GetString(0),
                        Gelir = reader.IsDBNull(1) ? 0 : reader.GetDouble(1),
                        Gider = reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                        Tutar = yearTotal
                    });
                    totalAmount += yearTotal;
                }
            }
            _connection.Close();
            return (summaries, totalAmount);
        }

        public (List<MonthlySummary> summaries, double total) GetMonthlySummaries(string year)
        {
            List<MonthlySummary> summaries = new List<MonthlySummary>();
            double totalAmount = 0;

            _connection.Open();
            string query = @"
        SELECT 
        strftime('%m', IslemTarihi) AS Ay, 
        SUM(CASE WHEN Tutar > 0 THEN Tutar ELSE 0 END) AS Gelir, 
        SUM(CASE WHEN Tutar < 0 THEN ABS(Tutar) ELSE 0 END) AS Gider,
        SUM(Tutar) AS Tutar
        FROM FinancialTransactions 
        WHERE strftime('%Y', IslemTarihi) = @Year
        GROUP BY Ay
        ORDER BY Ay ASC;";

            using (var command = new SQLiteCommand(query, _connection))
            {
                command.Parameters.AddWithValue("@Year", year);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        double monthTotal = reader.IsDBNull(3) ? 0 : reader.GetDouble(3);
                        summaries.Add(new MonthlySummary
                        {
                            Ay = reader.GetString(0),
                            Gelir = reader.IsDBNull(1) ? 0 : reader.GetDouble(1),
                            Gider = reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                            Tutar = monthTotal,
                            Yil = year
                        });
                        totalAmount += monthTotal;
                    }
                }
            }
            _connection.Close();

            return (summaries, totalAmount);
        }

        public (List<FinancialTransaction> transactions, double total) GetFinancialTransactions(string month, string year)
        {
            List<FinancialTransaction> transactions = new List<FinancialTransaction>();
            double totalAmount = 0;

            _connection.Open();
            string query = @"
        SELECT Id, strftime('%Y-%m-%d', IslemTarihi) AS IslemTarihi, Aciklama, Tutar
        FROM FinancialTransactions
        WHERE strftime('%Y', IslemTarihi) = @Year
        AND strftime('%m', IslemTarihi) = @Month
        ORDER BY IslemTarihi ASC;";

            using (var command = new SQLiteCommand(query, _connection))
            {
                command.Parameters.AddWithValue("@Year", year);
                command.Parameters.AddWithValue("@Month", month);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        double amount = reader.GetDouble(3);
                        transactions.Add(new FinancialTransaction
                        {
                            Id = reader.GetInt32(0),
                            IslemTarihi = reader.GetDateTime(1),
                            Aciklama = reader.GetString(2),
                            Tutar = amount
                        });
                        totalAmount += amount;
                    }
                }
            }
            _connection.Close();

            return (transactions, totalAmount);
        }


        public void DeleteYearlyData(string year)
        {
            var query = "DELETE FROM FinancialTransactions WHERE strftime('%Y', IslemTarihi) = @Year";

            using (var command = new SQLiteCommand(query, _connection))
            {
                _connection.Open();
                command.Parameters.AddWithValue("@Year", year);
                command.ExecuteNonQuery();
                _connection.Close();
            }
        }

        public void DeleteMonthlyData(string month, string year)
        {
            var query = "DELETE FROM FinancialTransactions WHERE strftime('%Y', IslemTarihi) = @Year AND strftime('%m', IslemTarihi) = @Month";

            using (var command = new SQLiteCommand(query, _connection))
            {
                _connection.Open();
                command.Parameters.AddWithValue("@Year", year);
                command.Parameters.AddWithValue("@Month", month.PadLeft(2, '0')); 
                command.ExecuteNonQuery();
                _connection.Close();
            }
        }
    }
}
