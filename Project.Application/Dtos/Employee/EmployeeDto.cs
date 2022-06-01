using System;

namespace Project.Application.Dtos.Employee
{
    public class EmployeeDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
