using System.ComponentModel.DataAnnotations;

namespace ProductApi.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public int? ProductTypeID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        public int Stock { get; set; }

        public string? Description { get; set; }
    }
}