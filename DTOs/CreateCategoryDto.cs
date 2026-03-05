using System.ComponentModel.DataAnnotations;

namespace WebApplication2.DTOs
{
    public class CreateCategoryDto
    {
        // ID теперь можно передавать (опционально)
        public int? Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}