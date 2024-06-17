using System;
using System.Collections.Generic;

namespace WebApartmentMangament.Models
{
    public partial class Apartment
    {
        public Apartment()
        {
            Accounts = new HashSet<Account>();
            ApartmentServices = new HashSet<ApartmentService>();
            Contracts = new HashSet<Contract>();
            ElectricMeters = new HashSet<ElectricMeter>();
            ResidentsRequireds = new HashSet<ResidentsRequired>();
            Revenues = new HashSet<Revenue>();
            WaterMeters = new HashSet<WaterMeter>();
        }

        public int ApartmentId { get; set; }
        public int? BuildingId { get; set; }
        public string? ApartmentCode { get; set; }
        public string? ApartmentName { get; set; }
        public int? ApartmentNumber { get; set; }
        public int? FloorNumber { get; set; }
        public DateTime? StartDay { get; set; }
        public double? Area { get; set; }
        public byte? Status { get; set; }

        public virtual Building? Building { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<ApartmentService> ApartmentServices { get; set; }
        public virtual ICollection<Contract> Contracts { get; set; }
        public virtual ICollection<ElectricMeter> ElectricMeters { get; set; }
        public virtual ICollection<ResidentsRequired> ResidentsRequireds { get; set; }
        public virtual ICollection<Revenue> Revenues { get; set; }
        public virtual ICollection<WaterMeter> WaterMeters { get; set; }
    }
}
