using System;
using System.Collections.Generic;

namespace WebApartmentMangament.Models
{
    public partial class ResidentsRequired
    {
        public int RequestId { get; set; }
        public int? AccountId { get; set; }
        public int? ApartmentId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? CreateDay { get; set; }
        public DateTime? FixDay { get; set; }
        public int? Pending { get; set; }
        public byte? Status { get; set; }

        public virtual Apartment? Apartment { get; set; }
    }
}
