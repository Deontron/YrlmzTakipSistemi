using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
