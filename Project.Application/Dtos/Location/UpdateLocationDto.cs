using Project.Core.Validations;
using System.ComponentModel.DataAnnotations;

namespace Project.Application.Dtos.Location
{
    public class UpdateLocationDto
    {
        [Required]
        [ValidId]
        public int Id { get; set; }
        public int? Floor { get; set; }
        public string? Cube { get; set; }
    }
}
