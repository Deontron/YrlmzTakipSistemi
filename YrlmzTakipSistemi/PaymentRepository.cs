using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Input;

namespace YrlmzTakipSistemi.Repositories
{
    public class PaymentRepository : Repository<Payment>
    {
        public PaymentRepository(SQLiteConnection connection) : base(connection, "Payments") { }

        public double GetTotalAmount()
        {
            List<Payment> payments = GetWithAValue("Tutar", "OdemeDurumu = 1 OR OdemeDurumu = 2");
            double total = 0;

            foreach(Payment payment in payments)
            {
                total += payment.Tutar;
            }
            return total;
        }

        public void ChangePaidState(Payment selectedPayment)
        {
                selectedPayment.OdemeDurumu++;

                if (selectedPayment.OdemeDurumu > 4)
                {
                    selectedPayment.OdemeDurumu = 1;
                }

                selectedPayment.OdemeDescription = GetPaidDescription(selectedPayment.OdemeDurumu);

            Update(selectedPayment);
        }

        private string GetPaidDescription(int state)
        {
            if (state == 1)
            {
                return "Ödenmedi";
            }
            else if (state == 2)
            {
                return "Bankada";
            }
            else if (state == 3)
            {
                return "Tahsil";
            }
            else if (state == 4)
            {
                return "Diğer";
            }
            else
            {
                return "Bilinmiyor";
            }
        }
    }
}
