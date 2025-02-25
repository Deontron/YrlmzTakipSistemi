using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YrlmzTakipSistemi
{
    internal class MyClasses
    {
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LongName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TaxNo { get; set; } = string.Empty;
        public string TaxOffice { get; set; } = string.Empty;
        public double Debt { get; set; }

    }

    public class Transaction
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int FaturaId { get; set; }
        public int OdemeId { get; set; }
        public int FinansalId { get; set; }
        public DateTime Tarih { get; set; }
        public string Aciklama { get; set; } = String.Empty;
        public string Notlar { get; set; } = String.Empty;
        public int Adet { get; set; }
        public double BirimFiyat { get; set; }
        public double Tutar { get; set; }
        public double Odenen { get; set; }
        public double AlacakDurumu { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime Tarih { get; set; }
        public string Isim { get; set; } = string.Empty;
        public double Fiyat { get; set; }
    }

    public class Payment
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime Tarih { get; set; }
        public string Musteri { get; set; } = string.Empty;
        public string Borclu { get; set; } = string.Empty;
        public string KasideYeri { get; set; } = string.Empty;
        public int Kategori { get; set; }
        public string KategoriDescription { get; set; } = string.Empty;
        public double Tutar { get; set; }
        public DateTime OdemeTarihi { get; set; }
        public int OdemeDurumu { get; set; }
        public string OdemeDescription { get; set; } = string.Empty;
    }

    public class Invoice
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime Tarih { get; set; } 
        public string Musteri { get; set; } = string.Empty;
        public string FaturaNo { get; set; } = string.Empty;
        public DateTime FaturaTarihi { get; set; } 
        public double Tutar { get; set; }
        public double KDV { get; set; }
        public double Toplam { get; set; }
    }

    public class FinancialTransaction
    {
        public int Id { get; set; }
        public string IslemTarihi { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public double Tutar { get; set; }
    }

    public class MonthlySummary
    {
        public string Ay { get; set; } = string.Empty;
        public string Yil { get; set; } = string.Empty;
        public double Gelir { get; set; }
        public double Gider { get; set; }
        public double Tutar { get; set; }
    }
    public class YearlySummary
    {
        public string Yil { get; set; } = string.Empty;
        public double Gelir { get; set; }
        public double Gider { get; set; }
        public double Tutar { get; set; }
    }
}
