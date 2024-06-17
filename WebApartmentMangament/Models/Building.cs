using System;
using System.Collections.Generic;

namespace WebApartmentMangament.Models
{
    public partial class Building
    {
        public Building()
        {
            Apartments = new HashSet<Apartment>();
        }

        public int BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public string? BuildingCode { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }
        public int? FloorNumber { get; set; }
        public int? ApartmentNumber { get; set; }
        public int? AccNumber { get; set; }
        public byte? Status { get; set; }

        public virtual ICollection<Apartment> Apartments { get; set; }
    }
}
