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

        public List<YearlySummary> GetYearlySummaries()
        {
            List<YearlySummary> summaries = new List<YearlySummary>();

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
                    summaries.Add(new YearlySummary
                    {
                        Yil = reader.GetString(0),
                        Gelir = reader.IsDBNull(1) ? 0 : reader.GetDouble(1),
                        Gider = reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                        Tutar = reader.IsDBNull(3) ? 0 : reader.GetDouble(3)
                    });
                }
            }
            _connection.Close();
            return summaries;
        }
        public List<MonthlySummary> GetMonthlySummaries(string year)
        {
            List<MonthlySummary> summaries = new List<MonthlySummary>();

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
                        summaries.Add(new MonthlySummary
                        {
                            Ay = reader.GetString(0),
                            Gelir = reader.IsDBNull(1) ? 0 : reader.GetDouble(1),
                            Gider = reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                            Tutar = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                            Yil = year
                        });
                    }
                }
            }
            _connection.Close();

            return summaries;
        }

        public List<FinancialTransaction> GetFinancialTransactions(string month, string year)
        {
            List<FinancialTransaction> transactions = new List<FinancialTransaction>();

            _connection.Open();
            string query = @"
                SELECT Id, strftime('%d-%m-%Y', IslemTarihi) AS IslemTarihi, Aciklama, Tutar
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
                        transactions.Add(new FinancialTransaction
                        {
                            Id = reader.GetInt32(0),
                            IslemTarihi = reader.GetString(1),
                            Aciklama = reader.GetString(2),
                            Tutar = reader.GetDouble(3)
                        });
                    }
                }
            }
            _connection.Close();

            return transactions;
        }
    }
}
