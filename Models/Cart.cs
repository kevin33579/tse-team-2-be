using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CartApi.Models
{
    [Table("carts")]
    public class Cart
    {
        // ---------- Columns ----------
        [Key]
        public uint Id { get; set; }  // INT UNSIGNED AUTO_INCREMENT

        public uint UserId { get; set; }

        public uint ProductId { get; set; }

        public uint? ScheduleId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
        public DateTime? ScheduleTime { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductTypeName { get; set; } = string.Empty;

        public string? ProductImageUrl { get; set; }

        public decimal ProductPrice { get; set; }


    }


}
