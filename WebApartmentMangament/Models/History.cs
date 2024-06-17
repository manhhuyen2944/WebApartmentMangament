using System;
using System.Collections.Generic;

namespace WebApartmentMangament.Models
{
    public partial class History
    {
        public int HistoryId { get; set; }
        public int? AccountId { get; set; }
        public DateTime? Day { get; set; }
        public string? Description { get; set; }
        public byte? Action { get; set; }
        public string? Screen { get; set; }

        public virtual Account? Account { get; set; }
    }
}
