using Project.Core.Common;
using Project.Core.Common.Enum;
using Project.Core.Validations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Core.Entities
{
    public class LostProperty : BaseEntity<int>
    {
        [Key]
        [ValidId]
        public override int Id { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }
        public PropertyStatus Status { get; set; }
        public int? LocationId { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? FoundTime { get; set; }
        public string? EmployeeId { get; set; }

        public Employee? Employee { get; set; }
        public Location? Location { get; set; }
    }
}
