using Project.Core.Common;
using Project.Core.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project.Core.Entities
{
    public class Employee : BaseEntity<string>
    {
        [Key]
        [MaxLength(40)]
        public override string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
        [Required]
        [MaxLength(15)]
        public string Phone { get; set; }
        [MaxLength(150)]
        public string? Email { get; set; }
        [DataType(DataType.Date)]
        [ValidDate]
        public DateTime DateOfBirth { get; set; }

        public IList<LostProperty> LostProperties { get; set; }
    }
}
