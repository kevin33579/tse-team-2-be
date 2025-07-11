using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using InvoiceDetailApi.Models;
using ProductApi.Exceptions;       // DatabaseException

namespace InvoiceDetailApi.Data
{
    public interface IInvoiceDetailRepository
    {
        Task<uint> CreateAsync(DetailInvoice detail, CancellationToken ct = default);
        Task<int> CreateManyAsync(IEnumerable<DetailInvoice> details, CancellationToken ct = default);
        Task<List<DetailInvoice>> GetByInvoiceIdAsync(uint invoiceId, CancellationToken ct = default);

        Task<List<InvoiceDetailSummaryDto>> GetByUserIdAsync(
    uint userId, CancellationToken ct = default);

    }

    public class InvoiceDetailRepository : IInvoiceDetailRepository
    {
        private readonly string _connString;

        public InvoiceDetailRepository(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException(
                              "Missing connection string: DefaultConnection");
        }

        // ───────────────────────────────
        //  CREATE (single row)
        // ───────────────────────────────
        public async Task<uint> CreateAsync(DetailInvoice detail, CancellationToken ct = default)
        {
            const string sql = @"
INSERT INTO detail_invoice (invoice_id, product_id, schedule_id)
VALUES (@InvoiceId, @ProductId, @ScheduleId);
SELECT LAST_INSERT_ID();";

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@InvoiceId", detail.InvoiceId);
                cmd.Parameters.AddWithValue("@ProductId", detail.ProductId);
                cmd.Parameters.AddWithValue("@ScheduleId", detail.ScheduleId);

                var newId = (ulong)await cmd.ExecuteScalarAsync(ct);
                return (uint)newId;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("INSERT",
                    $"Failed to create detail for invoice {detail.InvoiceId}.", ex);
            }
        }

        // ───────────────────────────────
        //  BULK‑CREATE (many rows)
        // ───────────────────────────────
        public async Task<int> CreateManyAsync(IEnumerable<DetailInvoice> details, CancellationToken ct = default)
        {
            var list = new List<DetailInvoice>(details);
            if (list.Count == 0) return 0;

            var values = new List<string>();
            for (int i = 0; i < list.Count; i++)
                values.Add($"(@Inv{i}, @Prod{i}, @Sched{i})");

            var insertSql = $@"
INSERT INTO detail_invoice (invoice_id, product_id, schedule_id)
VALUES {string.Join(", ", values)};";

            var updateSql = new List<string>();
            for (int i = 0; i < list.Count; i++)
                updateSql.Add($"UPDATE product SET stock = stock - 1 WHERE id = @Prod{i};");

            var fullSql = insertSql + "\n" + string.Join("\n", updateSql);

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var tx = await conn.BeginTransactionAsync(ct);
                await using var cmd = new MySqlCommand(fullSql, conn, (MySqlTransaction)tx);

                for (int i = 0; i < list.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"@Inv{i}", list[i].InvoiceId);
                    cmd.Parameters.AddWithValue($"@Prod{i}", list[i].ProductId);
                    cmd.Parameters.AddWithValue($"@Sched{i}", list[i].ScheduleId);
                }

                int rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
                await tx.CommitAsync(ct);

                return rowsAffected;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("INSERT+STOCK",
                    "Failed to bulk-insert detail_invoice rows and update stock.", ex);
            }
        }


        // ───────────────────────────────
        //  SELECT by invoice_id
        // ───────────────────────────────
        public async Task<List<DetailInvoice>> GetByInvoiceIdAsync(uint invoiceId, CancellationToken ct = default)
        {
            const string sql = @"
SELECT di.id,
       di.invoice_id,
       di.product_id,
       di.schedule_id,

       inv.invoiceCode,
       inv.date        AS invoiceDate,
       inv.totalPrice  AS invoiceTotalPrice,

       p.name          AS productName,
       p.price         AS productPrice,
       pt.name         AS productTypeName,

       s.time          AS scheduleTime            

FROM   detail_invoice di
JOIN   invoice       inv ON inv.id = di.invoice_id
JOIN   product        p  ON p.id  = di.product_id
JOIN   producttype    pt ON pt.id = p.productTypeId
LEFT JOIN schedule     s ON s.id  = di.schedule_id   
WHERE  di.invoice_id = @InvoiceId;";




            var result = new List<DetailInvoice>();

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);

                await using var rdr = (MySqlDataReader)await cmd.ExecuteReaderAsync(ct);
                while (await rdr.ReadAsync(ct))
                {
                    result.Add(new DetailInvoice
                    {
                        Id = rdr.GetUInt32("id"),
                        InvoiceId = rdr.GetUInt32("invoice_id"),
                        ProductId = rdr.GetUInt32("product_id"),
                        ScheduleId = rdr.GetUInt32("schedule_id"),
                        InvoiceCode = rdr.GetString("invoiceCode"),
                        InvoiceDate = rdr.GetDateTime("invoiceDate"),
                        InvoiceTotalPrice = rdr.GetDecimal("invoiceTotalPrice"),
                        ProductName = rdr.GetString("productName"),
                        ProductPrice = rdr.GetDecimal("productPrice"),
                        ProductTypeName = rdr.GetString("productTypeName"),
                        ScheduleTime = rdr.GetDateTime("scheduleTime")
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT",
                    $"Failed to fetch detail_invoice rows for invoice {invoiceId}.", ex);
            }
        }

        public async Task<List<InvoiceDetailSummaryDto>> GetByUserIdAsync(
    uint userId, CancellationToken ct = default)
        {
            const string sql = @"
SELECT  p.imageUrl     AS productImageUrl,
        pt.name        AS productTypeName,
        p.name         AS productName,
        s.time         AS scheduleTime
FROM    detail_invoice    di
JOIN    invoice           i  ON  i.id       = di.invoice_id
                              AND i.user_id = @UserId        -- user filter
JOIN    product           p  ON  p.id       = di.product_id
JOIN    producttype       pt ON  pt.id      = p.productTypeId
LEFT JOIN schedule        s  ON  s.id       = di.schedule_id
WHERE   DATE(s.time) >= CURDATE()                            -- ← today or future
ORDER BY s.time;  ";

            var list = new List<InvoiceDetailSummaryDto>();

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync(ct);

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await using var rdr = (MySqlDataReader)await cmd.ExecuteReaderAsync(ct);
            while (await rdr.ReadAsync(ct))
            {
                list.Add(new InvoiceDetailSummaryDto
                {
                    ProductImageUrl = rdr.IsDBNull("productImageUrl") ? string.Empty : rdr.GetString("productImageUrl"),
                    ProductTypeName = rdr.GetString("productTypeName"),
                    ProductName = rdr.GetString("productName"),
                    ScheduleTime = rdr.IsDBNull("scheduleTime") ? null : rdr.GetDateTime("scheduleTime")
                });
            }
            return list;
        }

    }

    internal static class MySqlDataReaderExtensions
    {
        public static uint GetUInt32(this MySqlDataReader rdr, string column) =>
            rdr.GetUInt32(rdr.GetOrdinal(column));

        public static int GetInt32(this MySqlDataReader rdr, string column) =>
            rdr.GetInt32(rdr.GetOrdinal(column));

        public static bool IsDBNull(this MySqlDataReader rdr, string column) =>
            rdr.IsDBNull(rdr.GetOrdinal(column));
    }




}
