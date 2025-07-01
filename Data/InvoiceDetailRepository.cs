using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using ProductApi.Exceptions;      // DatabaseException
using InvoiceDetailApi.Models;             // DetailInvoice model

namespace InvoiceDetailApi.Data
{
    public interface IInvoiceDetailRepository
    {
        /// <summary>Create a single detail row and return its new ID.</summary>
        Task<uint> CreateAsync(DetailInvoice detail, CancellationToken ct = default);

        /// <summary>Bulk‑insert many rows; returns number of rows inserted.</summary>
        Task<int> CreateManyAsync(IEnumerable<DetailInvoice> details, CancellationToken ct = default);

        /// <summary>Get all details for one invoice.</summary>
        Task<List<DetailInvoice>> GetByInvoiceIdAsync(uint invoiceId, CancellationToken ct = default);
    }

    public class InvoiceDetailRepository : IInvoiceDetailRepository
    {
        private readonly string _connString;

        public InvoiceDetailRepository(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException(
                          "Missing connection string: DefaultConnection");
        }

        // ───────────────────────────────────────────────────────
        // CREATE one row
        // ───────────────────────────────────────────────────────
        public async Task<uint> CreateAsync(DetailInvoice detail, CancellationToken ct = default)
        {
            const string sql = @"
INSERT INTO detail_invoice (invoice_id, cart_id)
VALUES (@InvoiceId, @CartId);
SELECT LAST_INSERT_ID();";

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@InvoiceId", detail.InvoiceId);
                cmd.Parameters.AddWithValue("@CartId", detail.CartId);

                var newId = (ulong)await cmd.ExecuteScalarAsync(ct);
                return (uint)newId;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("INSERT",
                    $"Gagal membuat detail_invoice untuk invoice {detail.InvoiceId}", ex);
            }
        }

        // ───────────────────────────────────────────────────────
        // BULK‑CREATE many rows
        // ───────────────────────────────────────────────────────
        public async Task<int> CreateManyAsync(IEnumerable<DetailInvoice> details, CancellationToken ct = default)
        {
            var list = new List<DetailInvoice>(details);
            if (list.Count == 0) return 0;

            // Build one VALUES set per row:  (@i0,@c0),(@i1,@c1)…
            var values = new List<string>();
            for (int i = 0; i < list.Count; i++)
                values.Add($"(@InvoiceId{i}, @CartId{i})");

            var sql = $@"
INSERT INTO detail_invoice (invoice_id, cart_id)
VALUES {string.Join(", ", values)};";

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);

                for (int i = 0; i < list.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"@InvoiceId{i}", list[i].InvoiceId);
                    cmd.Parameters.AddWithValue($"@CartId{i}", list[i].CartId);
                }

                return await cmd.ExecuteNonQueryAsync(ct);   // rows inserted
            }
            catch (Exception ex)
            {
                throw new DatabaseException("INSERT",
                    "Gagal membuat banyak detail_invoice", ex);
            }
        }

        // ───────────────────────────────────────────────────────
        // GET by invoice_id
        // ───────────────────────────────────────────────────────
        public async Task<List<DetailInvoice>> GetByInvoiceIdAsync(uint invoiceId, CancellationToken ct = default)
        {
            const string sql = @"
SELECT  di.id,
        di.invoice_id,
        di.cart_id,

        p.id            AS product_id,
        p.name          AS productName,
        p.price         AS productPrice,
        pt.name         AS productTypeName,

        inv.invoiceCode,
        inv.totalPrice  AS invoiceTotalPrice,
        inv.date        AS invoiceDate,

        c.schedule_id,
        s.time          AS scheduleTime

FROM        detail_invoice di
JOIN        invoice       inv ON inv.id = di.invoice_id         
JOIN        carts          c  ON c.id  = di.cart_id
JOIN        product        p  ON p.id  = c.product_id
JOIN        producttype    pt ON pt.id = p.productTypeId
LEFT JOIN   schedule       s  ON s.id  = c.schedule_id
WHERE       di.invoice_id = @InvoiceId;";



            var result = new List<DetailInvoice>();

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);

                await using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    result.Add(new DetailInvoice
                    {
                        Id = reader.GetUInt32("id"),
                        InvoiceId = reader.GetUInt32("invoice_id"),
                        CartId = reader.GetUInt32("cart_id"),
                        ProductId = reader.GetUInt32("product_id"),
                        ProductName = reader.GetString("productName"),

                        ScheduleId = reader.GetUInt32("schedule_id"),
                        ScheduleTime = reader.GetDateTime("scheduleTime"),
                        ProductPrice = reader.GetDecimal("productPrice"),
                        ProductTypeName = reader.GetString("productTypeName"),
                        InvoiceCode = reader.GetString("invoiceCode"),
                        InvoiceTotalPrice = reader.GetDecimal("invoiceTotalPrice"),
                        InvoiceDate = reader.GetDateTime("invoiceDate"),
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT",
                    $"Gagal mengambil detail_invoice untuk invoice {invoiceId}", ex);
            }
        }
    }
}
