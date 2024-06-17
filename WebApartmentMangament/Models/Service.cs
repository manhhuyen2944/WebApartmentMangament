using System;
using System.Collections.Generic;

namespace WebApartmentMangament.Models
{
    public partial class Service
    {
        public Service()
        {
            ApartmentServices = new HashSet<ApartmentService>();
        }

        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string? Description { get; set; }
        public decimal? ServiceFee { get; set; }
        public byte? Status { get; set; }

        public virtual ICollection<ApartmentService> ApartmentServices { get; set; }
    }
}
