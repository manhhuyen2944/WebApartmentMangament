
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApartmentMangament.Models
{
    public partial class News
    {
        public int NewsId { get; set; }
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Image { get; set; }
        [Column(TypeName = "ntext")]
        public string? Description { get; set; }
        public DateTime? CreateDay { get; set; }
        public byte? Status { get; set; }
    }
}
