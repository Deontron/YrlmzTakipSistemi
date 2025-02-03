using System;
using System.Collections.Generic;
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
        public string Tarih { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public double Debt { get; set; }
        
    }

    public class Transaction
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Tarih { get; set; }
        public String Aciklama { get; set; }
        public String Notlar { get; set; }
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
        public string Tarih { get; set; }
        public string Isim { get; set; }
        public double Fiyat { get; set; }
    }

    public class Payment
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Tarih { get; set; }
        public string Musteri { get; set; }
        public string Borclu { get; set; }
        public string KasideYeri { get; set; }
        public int Kategori { get; set; }
        public string KategoriDescription { get; set; }
        public double Tutar { get; set; }
        public string OdemeTarihi { get; set; }
        public int OdemeDurumu { get; set; }
        public string OdemeDescription { get; set; }
    }
}
