using System.Data.SQLite;

namespace YrlmzTakipSistemi.Repositories
{
    public class ProductRepository : Repository<Product>
    {
        public ProductRepository(SQLiteConnection connection) : base(connection, "Products") { }

        public List<Product> GetByCustomerId(int customerId)
        {
            return GetAll($"CustomerId = {customerId}");
        }

        public void DeleteByCustomerId(int customerId)
        {
            var query = $"DELETE FROM Products WHERE CustomerId = @customerId";

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
