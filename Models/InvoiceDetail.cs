using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceDetailApi.Models        // adjust namespace if needed
{
    [Table("detail_invoice")]
    public class DetailInvoice
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }                 // AUTO_INCREMENT PK

        // ── invoice FK ─────────────────────────────────────────────
        [Column("invoice_id")]
        [Required]
        public uint InvoiceId { get; set; }

        // ── product FK ─────────────────────────────────────────────
        [Column("product_id")]
        [Required]
        public uint ProductId { get; set; }

        // ── schedule FK (nullable) ─────────────────────────────────
        [Column("schedule_id")]
        public uint? ScheduleId { get; set; }

        // DetailInvoice.cs  (only the new properties shown)
        [NotMapped] public string InvoiceCode { get; set; } = string.Empty;
        [NotMapped] public DateTime InvoiceDate { get; set; }
        [NotMapped] public decimal? InvoiceTotalPrice { get; set; }

        // DetailInvoice.cs  – append these
        [NotMapped] public string ProductName { get; set; } = string.Empty;
        [NotMapped] public decimal ProductPrice { get; set; }
        [NotMapped] public string ProductTypeName { get; set; } = string.Empty;

        [NotMapped] public DateTime? ScheduleTime { get; set; }   // already present? keep it





    }
    public class InvoiceDetailSummaryDto
    {
        public string ProductImageUrl { get; set; } = string.Empty;
        public string ProductTypeName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public DateTime? ScheduleTime { get; set; }
    }
}
