using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ProductApi.Exceptions;      // DatabaseException
using InvoiceApi.Models;          // Invoice model (rename namespace if needed)

namespace InvoiceApi.Data
{
    public interface IInvoiceRepository
    {
        Task<uint> CreateAsync(Invoice invoice, CancellationToken ct = default);
        Task<List<Invoice>> GetByUserIdAsync(uint userId, CancellationToken ct = default);
    }

    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly string _connString;
        public InvoiceRepository(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                             ?? throw new InvalidOperationException(
                                  "Missing connection string: DefaultConnection");
        }

        // ---------- INSERT ----------
        // INSERT + auto‑generate invoiceCode
        public async Task<uint> CreateAsync(Invoice invoice, CancellationToken ct = default)
        {
            // 1️⃣ Build a unique code:  INV‑20250701‑<4‑char random>
            // InvoiceRepository.CreateAsync
            var invoiceCode = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4]}";


            const string sql = @"
INSERT INTO invoice
        (user_id, invoiceCode, totalPrice, totalCourse, paymentMethodId)
VALUES  (@UserId, @InvoiceCode, @TotalPrice, @TotalCourse, @PaymentMethodId);
SELECT LAST_INSERT_ID();";           // return new PK

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", invoice.UserId);
                cmd.Parameters.AddWithValue("@InvoiceCode", invoiceCode);
                cmd.Parameters.AddWithValue("@TotalPrice", invoice.TotalPrice);
                cmd.Parameters.AddWithValue("@TotalCourse", invoice.TotalCourse);
                cmd.Parameters.AddWithValue("@PaymentMethodId", invoice.PaymentMethodId);

                var newId = (ulong)await cmd.ExecuteScalarAsync(ct);

                // 2️⃣ Optionally push the generated code back to the caller object
                invoice.Id = (uint)newId;
                invoice.InvoiceCode = invoiceCode;

                return (uint)newId;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("INSERT",
                    "Gagal membuat invoice baru", ex);
            }
        }


        // ---------- SELECT by user ----------
        public async Task<List<Invoice>> GetByUserIdAsync(uint userId, CancellationToken ct = default)
        {
            const string sql = @"
SELECT id, user_id, invoiceCode, `date`,
       totalPrice, totalCourse, paymentMethodId
FROM   invoice
WHERE  user_id = @UserId
ORDER  BY `date` DESC;";

            var invoices = new List<Invoice>();

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                await using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync(ct);

                while (await reader.ReadAsync(ct))
                {
                    invoices.Add(new Invoice
                    {
                        Id = reader.GetUInt32("id"),
                        UserId = reader.GetUInt32("user_id"),
                        InvoiceCode = reader.GetString("invoiceCode"),
                        Date = reader.GetDateTime("date"),
                        TotalPrice = reader.GetDecimal("totalPrice"),
                        TotalCourse = reader.GetInt32("totalCourse"),
                        PaymentMethodId = reader.GetInt32("paymentMethodId")
                    });
                }

                return invoices;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT",
                    $"Gagal mengambil invoice untuk user_id {userId}", ex);
            }
        }
    }
}
