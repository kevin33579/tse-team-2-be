using System.ComponentModel.DataAnnotations;

namespace ProductTypeApi.Models
{
    public class ProductType
    {
        public int Id { get; set; }
        [Required]

        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }







    }
}