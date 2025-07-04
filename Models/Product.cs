using System.ComponentModel.DataAnnotations;

namespace ProductApi.Models
{
    public class Product
    {
        public int id { get; set; }
        public int? productTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string name { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal price { get; set; }

        public int stock { get; set; }

        public string? description { get; set; }
        public string? imageUrl { get; set; }

        public string? productTypeName { get; set; }

    }

    public class ProductDto
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public decimal price { get; set; }
        public int stock { get; set; }
        public string? description { get; set; }
        public string? imageUrl { get; set; }
        public int? productTypeId { get; set; }
        public string productTypeName { get; set; } = string.Empty;
    }
}