using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using CartApi.Models;

namespace CartApi.Data
{
    /// <summary>
    /// Repository contract and implementation for carts, combined in one file.
    /// </summary>
    public interface ICartRepository
    {
        /// <summary>
        /// Returns all cart rows owned by <paramref name="userId"/>.
        /// </summary>
        Task<List<Cart>> GetCartsByUserAsync(uint userId, CancellationToken ct = default);
        Task<uint> CreateCartAsync(Cart cart, CancellationToken ct = default);
        Task<bool> DeleteCartAsync(uint id, CancellationToken ct = default);
    }

    public class CartRepository : ICartRepository
    {
        private readonly string _connectionString;

        public CartRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing connection string: DefaultConnection");
        }

        public async Task<List<Cart>> GetCartsByUserAsync(uint userId, CancellationToken ct = default)
        {
            const string sql = @"
SELECT  c.id,
        c.user_id,
        c.product_id,
        c.schedule_id,
        c.quantity,

        s.time              AS scheduleTime,
        p.name              AS productName,
        p.imageUrl          AS productImageUrl, 
         p.price             AS productPrice,  
        pt.name             AS productTypeName
FROM    carts c
JOIN    product       p  ON p.id  = c.product_id
JOIN    producttype   pt ON pt.id = p.productTypeId
LEFT JOIN schedule     s ON s.id  = c.schedule_id
WHERE   c.user_id = @userId";

            var carts = new List<Cart>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            // ──► Cast to MySqlDataReader so the extension methods are in scope
            await using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                carts.Add(new Cart
                {
                    Id = reader.GetUInt32("id"),
                    UserId = reader.GetUInt32("user_id"),
                    ProductId = reader.GetUInt32("product_id"),
                    ScheduleId = reader.IsDBNull("schedule_id") ? null : reader.GetUInt32("schedule_id"),
                    Quantity = reader.GetInt32("quantity"),
                    ScheduleTime = reader.IsDBNull("scheduleTime")
                            ? (DateTime?)null
                            : reader.GetDateTime("scheduleTime"),
                    ProductName = reader.GetString("productName"),
                    ProductTypeName = reader.GetString("productTypeName"),
                    ProductImageUrl = reader.IsDBNull("productImageUrl") ? null : reader.GetString("productImageUrl"),
                    ProductPrice = reader.GetDecimal("productPrice")
                });
            }

            return carts;
        }

        public async Task<uint> CreateCartAsync(Cart cart, CancellationToken ct = default)
        {
            const string sql = @"
                INSERT INTO carts (user_id, product_id, schedule_id, quantity)
                VALUES (@userId, @productId, @scheduleId, @quantity);";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", cart.UserId);
            cmd.Parameters.AddWithValue("@productId", cart.ProductId);
            cmd.Parameters.AddWithValue(
                "@scheduleId",
                cart.ScheduleId.HasValue ? cart.ScheduleId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@quantity", cart.Quantity);

            await cmd.ExecuteNonQueryAsync(ct);

            // MySqlCommand exposes the auto-increment value:
            var newId = (uint)cmd.LastInsertedId;
            return newId;
        }

        public async Task<bool> DeleteCartAsync(uint id, CancellationToken ct = default)
        {
            const string sql = "DELETE FROM carts WHERE id = @id;";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            int affected = await cmd.ExecuteNonQueryAsync(ct);
            return affected > 0;          // true → row deleted, false → nothing matched
        }

    }

    /// <summary>
    /// Convenience extensions so we can read columns by name instead of ordinal.
    /// </summary>
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
