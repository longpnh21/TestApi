using Project.Core.Validations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Application.Dtos.Employee
{
    public class CreateEmployeeDto
    {
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
        [Required]
        [DataType(DataType.Date)]
        [ValidDate]
        public DateTime DateOfBirth { get; set; }
    }
}
