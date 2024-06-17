using System;
using System.Collections.Generic;

namespace WebApartmentMangament.Models
{
    public partial class Account
    {
        public Account()
        {
            Histories = new HashSet<History>();
        }

        public int AccountId { get; set; }
        public int? ApartmentId { get; set; }
        public string? Code { get; set; }
        public string? Avartar { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public int? InfoId { get; set; }
        public int? RoleId { get; set; }
        public int? RelationshipId { get; set; }
        public byte? Status { get; set; }

        public virtual Apartment? Apartment { get; set; }
        public virtual InFo? Info { get; set; }
        public virtual Relationship? Relationship { get; set; }
        public virtual Role? Role { get; set; }
        public virtual ICollection<History> Histories { get; set; }
    }
}
