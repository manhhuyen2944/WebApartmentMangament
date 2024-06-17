using System;
using System.Collections.Generic;

namespace WebApartmentMangament.Models
{
    public partial class InFo
    {
        public InFo()
        {
            Accounts = new HashSet<Account>();
        }

        public int InfoId { get; set; }
        public string? FullName { get; set; }
        public DateTime? BirthDay { get; set; }
        public byte? Sex { get; set; }
        public string? CmndCccd { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? StreetAddress { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
    }
}
