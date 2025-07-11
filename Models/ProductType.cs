using System.ComponentModel.DataAnnotations;

namespace ProductTypeApi.Models
{
    public class ProductType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; }

    }
}