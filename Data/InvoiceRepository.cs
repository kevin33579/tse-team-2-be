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
        Task<Invoice> UpdateInvoiceAsync(Invoice invoice, CancellationToken ct = default);
        Task DeleteInvoiceAsync(uint id, CancellationToken ct = default);
        Task<List<Invoice>> SearchInvoiceAsync(string invoiceCode, CancellationToken ct = default);
        Task<List<Invoice>> GetAllWithNameAsync(CancellationToken ct = default);
        Task<Invoice?> GetByIdAsync(uint id, CancellationToken ct = default);
        Task<bool> DeleteAsync(uint id, CancellationToken ct = default);
        Task<Invoice> UpdateAsync(Invoice invoice, CancellationToken ct = default);

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
        // ---------- UPDATE Invoice ----------
        public async Task<Invoice> UpdateInvoiceAsync(Invoice invoice, CancellationToken ct = default)
        {
            const string sql = @"
UPDATE invoice
SET    user_id = @UserId,
       invoiceCode = @InvoiceCode,
       totalPrice = @TotalPrice,
       totalCourse = @TotalCourse,
       paymentMethodId = @PaymentMethodId
WHERE  id = @Id;";            // return new PK

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", invoice.Id);
                cmd.Parameters.AddWithValue("@UserId", invoice.UserId);
                cmd.Parameters.AddWithValue("@InvoiceCode", invoice.InvoiceCode);
                cmd.Parameters.AddWithValue("@TotalPrice", invoice.TotalPrice);
                cmd.Parameters.AddWithValue("@TotalCourse", invoice.TotalCourse);
                cmd.Parameters.AddWithValue("@PaymentMethodId", invoice.PaymentMethodId);

                await cmd.ExecuteNonQueryAsync(ct);

                return invoice;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("UPDATE",
                    "Gagal memperbarui data invoice", ex);
            }
        }
        // ---------- DELETE Invoice ----------
        public async Task DeleteInvoiceAsync(uint id, CancellationToken ct = default)
        {
            const string sql = @"
DELETE FROM invoice
WHERE  id = @Id;";

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                await cmd.ExecuteNonQueryAsync(ct);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("DELETE",
                    "Gagal menghapus data invoice", ex);
            }
        }
        // ---------- SEARCH Invoice by code ----------
        public async Task<List<Invoice>> SearchInvoiceAsync(string invoiceCode, CancellationToken ct = default)
        {
            const string sql = @"
    SELECT id, user_id, invoiceCode, `date`,
           totalPrice, totalCourse, paymentMethodId
    FROM   invoice
    WHERE  invoiceCode = @InvoiceCode;";

            var result = new List<Invoice>();

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@InvoiceCode", invoiceCode);

                await using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync(ct);

                while (await reader.ReadAsync(ct))
                {
                    result.Add(new Invoice
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

                return result;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT",
                    $"Gagal mencari invoice dengan kode {invoiceCode}", ex);
            }
        }

        // ---------- GET ALL Invoices ----------
        public async Task<List<Invoice>> GetAllWithNameAsync(CancellationToken ct = default)
        {
            const string sql = @"
SELECT i.id, i.user_id, i.invoiceCode, i.`date`,
       i.totalPrice, i.totalCourse, i.paymentMethodId,
       u.username AS userName,
       pm.name AS paymentMethodName
FROM   invoice i
JOIN   users u ON i.user_id = u.id
JOIN   paymentMethod pm ON i.paymentMethodId = pm.id
ORDER  BY i.`date` DESC;";

            var invoices = new List<Invoice>();

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);

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
                        PaymentMethodId = reader.GetInt32("paymentMethodId"),
                        UserName = reader.GetString("userName"),
                        PaymentMethodName = reader.GetString("paymentMethodName")
                    });
                }

                return invoices;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT",
                    "Gagal mengambil data invoice dengan user dan payment method", ex);
            }
        }

        // 🔍 Get invoice by ID
        public async Task<Invoice?> GetByIdAsync(uint id, CancellationToken ct = default)
        {
            const string sql = @"
SELECT id, user_id, invoiceCode, `date`,
       totalPrice, totalCourse, paymentMethodId
FROM   invoice
WHERE  id = @Id;";

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                await using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync(ct);

                if (await reader.ReadAsync(ct))
                {
                    return new Invoice
                    {
                        Id = reader.GetUInt32("id"),
                        UserId = reader.GetUInt32("user_id"),
                        InvoiceCode = reader.GetString("invoiceCode"),
                        Date = reader.GetDateTime("date"),
                        TotalPrice = reader.GetDecimal("totalPrice"),
                        TotalCourse = reader.GetInt32("totalCourse"),
                        PaymentMethodId = reader.GetInt32("paymentMethodId")
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT",
                    $"Gagal mengambil invoice dengan ID {id}", ex);
            }
        }

        // 🗑 Delete invoice (returns bool)
        public async Task<bool> DeleteAsync(uint id, CancellationToken ct = default)
        {
            const string sqlDeleteDetails = @"DELETE FROM detail_invoice WHERE invoice_id = @Id;";
            const string sqlDeleteInvoice = @"DELETE FROM invoice WHERE id = @Id;";

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                // Start a transaction to ensure atomicity
                await using var transaction = await conn.BeginTransactionAsync(ct);

                // Delete child detail_invoice rows first
                await using (var cmdDeleteDetails = new MySqlCommand(sqlDeleteDetails, conn, transaction))
                {
                    cmdDeleteDetails.Parameters.AddWithValue("@Id", id);
                    await cmdDeleteDetails.ExecuteNonQueryAsync(ct);
                }

                // Delete the invoice now
                int rowsAffected;
                await using (var cmdDeleteInvoice = new MySqlCommand(sqlDeleteInvoice, conn, transaction))
                {
                    cmdDeleteInvoice.Parameters.AddWithValue("@Id", id);
                    rowsAffected = await cmdDeleteInvoice.ExecuteNonQueryAsync(ct);
                }

                // Commit transaction
                await transaction.CommitAsync(ct);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("DELETE",
                    $"Gagal menghapus invoice dengan ID {id}", ex);
            }
        }


        // ✏️ Update invoice
        public Task<Invoice> UpdateAsync(Invoice invoice, CancellationToken ct = default)
        {
            return UpdateInvoiceAsync(invoice, ct); // delegate ke method yang sudah ada
        }



    }
}
