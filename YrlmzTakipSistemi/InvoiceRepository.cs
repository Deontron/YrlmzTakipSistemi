using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YrlmzTakipSistemi.Repositories;
using System.Data.SQLite;

namespace YrlmzTakipSistemi
{
    class InvoiceRepository : Repository<Invoice>
    {
        public InvoiceRepository(SQLiteConnection connection) : base(connection, "Invoices") { }

        public double GetTotalAmount()
        {
            string query = $"SELECT SUM(Toplam) FROM Invoices";

            try
            {
                using (var command = new SQLiteCommand(query, _connection))
                {
                    _connection.Open();
                    object result = command.ExecuteScalar();
                    _connection.Close();

                    return result != DBNull.Value && result != null ? Convert.ToDouble(result) : 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Toplam tutar alınırken hata oluştu: " + ex.Message, ex);
            }
        }
    }
}
