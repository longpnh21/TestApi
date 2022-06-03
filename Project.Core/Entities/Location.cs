using Project.Core.Common;
using Project.Core.Validations;
using System.ComponentModel.DataAnnotations;

namespace Project.Core.Entities
{
    public class Location : BaseEntity<int>
    {
        [Key]
        [ValidId]
        public override int Id { get; set; }
        public int? Floor { get; set; }
        [MaxLength(150)]
        public string? Cube { get; set; }
    }
}
