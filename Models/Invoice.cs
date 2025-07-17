using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceApi.Models
{
    [Table("invoice")]
    public class Invoice
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }


        [Column("invoiceCode")]
        [MaxLength(100)]
        public string InvoiceCode { get; set; } = string.Empty;

        [Column("date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Column("totalPrice", TypeName = "decimal(10,2)")]
        public decimal? TotalPrice { get; set; }   // nullable because SQL column is nullable

        [Column("totalCourse")]
        public int? TotalCourse { get; set; }      // nullable for the same reason

        [Column("paymentMethodId")]
        public int? PaymentMethodId { get; set; }  // nullable if FK can be null

        [Column("user_id")]
        public uint UserId { get; set; }

        [NotMapped]
        public string? UserName { get; set; }

        [NotMapped]
        public string? PaymentMethodName { get; set; }
        [NotMapped]
        public List<InvoiceDetailApi.Models.DetailInvoice> Details { get; set; } = new();

    }
}
