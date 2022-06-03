using Project.Core.Common.Enum;
using Project.Core.Validations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Application.Dtos.LostProperty
{
    public class LostPropertyDto
    {
        [ValidId]
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PropertyStatus Status { get; set; }
        public int? LocationId { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? FoundTime { get; set; }
        public string? EmployeeId { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? LastModifiedTime { get; set; }
    }
}
