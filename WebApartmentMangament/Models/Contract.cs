using System;
using System.Collections.Generic;

namespace WebApartmentMangament.Models
{
    public partial class Contract
    {
        public int ContractId { get; set; }
        public int? ApartmentId { get; set; }
        public int? AccountId { get; set; }
        public DateTime? StartDay { get; set; }
        public DateTime? EndDay { get; set; }
        public decimal? MonthlyRent { get; set; }
        public decimal? Deposit { get; set; }
        public byte? Status { get; set; }

        public virtual Apartment? Apartment { get; set; }
        public virtual Account? Account { get; set; }
    }
}
