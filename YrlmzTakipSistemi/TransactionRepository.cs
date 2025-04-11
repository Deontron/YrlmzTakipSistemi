using System.Data.SQLite;

namespace YrlmzTakipSistemi.Repositories
{
    public class TransactionRepository : Repository<Transaction>
    {
        public TransactionRepository(SQLiteConnection connection) : base(connection, "Transactions") { }

        public List<Transaction> GetByCustomerId(int customerId)
        {
            return GetAll($"CustomerId = {customerId}");
        }

        public double GetTotalDebtByCustomerId(int customerId)
        {
            double totalDebt = 0;

            using (var command = new SQLiteCommand($"SELECT SUM(AlacakDurumu) FROM Transactions WHERE CustomerId = @CustomerId", _connection))
            {
                _connection.Open();
                command.Parameters.AddWithValue("@CustomerId", customerId);

                var result = command.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    totalDebt = Convert.ToDouble(result);
                }
                _connection.Close();
            }

            return totalDebt;
        }

        public void DeleteWithDoc(int id, string doc)
        {
            var allowedColumns = new HashSet<string> { "FaturaId", "OdemeId", "FinansalId" };

            if (!allowedColumns.Contains(doc))
            {
                throw new ArgumentException("Geçersiz sütun adı!");
            }

            var query = $"DELETE FROM Transactions WHERE {doc} = @Id";

            try
            {
                using (var command = new SQLiteCommand(query, _connection))
                {
                    _connection.Open();
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Veri silinirken bir hata oluştu.", ex);
            }
            finally
            {
                _connection.Close();
            }
        }

        public void DeleteByCustomerId(int customerId)
        {
            var query = $"DELETE FROM Transactions WHERE CustomerId = @customerId";

            try
            {
                using (var command = new SQLiteCommand(query, _connection))
                {
                    _connection.Open();
                    command.Parameters.AddWithValue("@customerId", customerId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Veri silinirken bir hata oluştu.", ex);
            }
            finally
            {
                _connection.Close();
            }
        }
    }
}