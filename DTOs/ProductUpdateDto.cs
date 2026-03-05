using System.ComponentModel.DataAnnotations;

namespace WebApplication2.DTOs
{
    public class ProductUpdateDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Price { get; set; }

        public int? CategoryId { get; set; }
    }
}