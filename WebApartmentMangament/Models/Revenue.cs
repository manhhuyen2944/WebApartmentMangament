using System;
using System.Collections.Generic;

namespace WebApartmentMangament.Models
{
    public partial class Revenue
    {
        public int RevenueId { get; set; }
        public int? ApartmentId { get; set; }
        public decimal? TotalMoney { get; set; }
        public decimal? Pay { get; set; }
        public decimal? Debt { get; set; }
        public decimal? ServiceFee { get; set; }
        public string? CodeVoucher { get; set; }
        public double? WaterNumber { get; set; }
        public double? ElectricNumber { get; set; }
        public DateTime? DayCreat { get; set; }
        public DateTime? DayPay { get; set; }
        public byte? Payments { get; set; }
        public int? AccountId { get; set; }
        public byte? Status { get; set; }

        public virtual Apartment? Apartment { get; set; }
        public virtual Account? Account { get; set; }
    }
}
