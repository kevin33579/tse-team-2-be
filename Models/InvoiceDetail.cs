using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceDetailApi.Models          // ⬅️  adjust to your project namespace
{
    [Table("detail_invoice")]
    public class DetailInvoice
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }               // AUTO_INCREMENT PK

        [Column("invoice_id")]
        [Required]
        public uint InvoiceId { get; set; }        // FK → invoice.id

        [Column("cart_id")]
        [Required]
        public uint CartId { get; set; }           // FK → carts.id

        public uint ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public uint? ScheduleId { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductTypeName { get; set; } = string.Empty;

        public string InvoiceCode { get; set; } = string.Empty;
        public decimal? InvoiceTotalPrice { get; set; }
        public DateTime InvoiceDate { get; set; }


        /* optional navigation properties
        public Invoice Invoice { get; set; }       // if you use EF Core
        public Cart    Cart    { get; set; }
        */
    }
}
