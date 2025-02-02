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
        public string Name { get; set; }
        public string Email { get; set; }
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
        public string Aciklama { get; set; }
        public double Miktar { get; set; }
        public int Kategori { get; set; }
        public bool OdendiMi { get; set; }
    }
}
