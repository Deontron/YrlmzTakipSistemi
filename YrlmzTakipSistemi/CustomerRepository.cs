﻿using System.Data.SQLite;

namespace YrlmzTakipSistemi.Repositories
{
    public class CustomerRepository : Repository<Customer>
    {
        public CustomerRepository(SQLiteConnection connection) : base(connection, "Customers") { }

        public double GetCustomerDebtById(int customerId)
        {
            try
            {
                var customer = GetById(customerId); 

                if (customer == null)
                {
                    return 0; 
                }

                return Convert.ToDouble(customer.Debt);
            }
            catch (Exception ex)
            {
                throw new Exception("Borç bilgisi alınırken hata oluştu: " + ex.Message, ex);
            }
        }

        public double GetTotalDebt()
        {
            double totalDebt = 0;

            using (var connection = new SQLiteConnection(_connection.ConnectionString))
            {
                connection.Open();

                string query = "SELECT SUM(Debt) FROM Customers";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        totalDebt = Convert.ToDouble(result);
                    }
                }
            }

            return totalDebt;
        }

        public void UpdateCustomerDebtById(double amount, int customerId)
        {
            Customer customer = GetById(customerId);

            if (customer == null)
            {
                return; 
            }

            double totalDebt = GetCustomerDebtById(customerId);
            totalDebt += amount;

            customer.Debt = totalDebt;
            Update(customer);
        }
    }
}
