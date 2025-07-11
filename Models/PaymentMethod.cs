using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PaymentApi.Models
{
    [Table("paymentMethod")]
    public class PaymentMethod
    {
        // ---------- Columns ----------
        [Key]
        public uint Id { get; set; }  // INT UNSIGNED AUTO_INCREMENT

        public string Name { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;




    }


}
