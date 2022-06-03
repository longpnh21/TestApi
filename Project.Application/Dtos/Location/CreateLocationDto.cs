using System.ComponentModel.DataAnnotations;

namespace Project.Application.Dtos.Location
{
    public class CreateLocationDto
    {
        public int? Floor { get; set; }
        [MaxLength(150)]
        public string? Cube { get; set; }
    }
}
