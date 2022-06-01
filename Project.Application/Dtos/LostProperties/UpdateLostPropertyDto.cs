﻿using Project.Core.Common.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Application.Dtos.LostProperties
{
    public class UpdateLostPropertyDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }
        public PropertyStatus Status { get; set; }
        [MaxLength(500)]
        public string? Location { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? FoundTime { get; set; }
        public string? EmployeeId { get; set; }
    }
}
