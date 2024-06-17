using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApartmentMangament.Models
{
    public partial class WaterMeter
    {
        public int WaterMeterId { get; set; }
        public int? ApartmentId { get; set; }
        [Column(TypeName = "datetime2(7)")]
        public DateTime? RegistrationDate { get; set; }
        public string? Code { get; set; }
        [Column(TypeName = "datetime2(7)")]
        public DateTime? DeadingDate { get; set; }
        public double? NumberOne { get; set; }
        public double? NumberEnd { get; set; }
        public decimal? Price { get; set; }
        public byte? Status { get; set; }

        public virtual Apartment? Apartment { get; set; }
    }
}
