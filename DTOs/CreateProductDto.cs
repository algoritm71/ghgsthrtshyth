using System.ComponentModel.DataAnnotations;

namespace WebApplication2.DTOs
{
    public class CreateProductDto
    {
        // ID теперь можно передавать (опционально)
        public int? Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}