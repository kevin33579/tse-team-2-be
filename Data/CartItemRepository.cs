using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using CartItemApi.Models;            // assumes you have CartItem class in this namespace

namespace CartItemApi.Data
{
    /* ---------- Interface ---------- */
    public interface ICartItemRepository
    {
        /// <summary>
        /// Inserts a new row into cart_items and returns the new Id.
        /// </summary>
        Task<int> CreateAsync(CartItem item);
        Task<List<CartItemView>> GetByUserIdAsync(int userId);
    }

    /* ---------- Implementation ---------- */
    public class CartItemRepository : ICartItemRepository
    {
        private readonly string _connectionString;

        public CartItemRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        public async Task<int> CreateAsync(CartItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                INSERT INTO cart_items (cart_id, product_id, schedule_id, quantity)
                VALUES (@cartId, @productId, @scheduleId, @quantity);";

            await using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@cartId", item.CartId);
            cmd.Parameters.AddWithValue("@productId", item.ProductId);

            // schedule_id may be NULL
            cmd.Parameters.AddWithValue("@scheduleId",
                item.ScheduleId.HasValue ? item.ScheduleId.Value : (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@quantity", item.Quantity);

            await cmd.ExecuteNonQueryAsync();

            // MySqlCommand.LastInsertedId returns the AUTO_INCREMENT value
            return (int)cmd.LastInsertedId;
        }

        public async Task<List<CartItemView>> GetByUserIdAsync(int userId)
        {
            var list = new List<CartItemView>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
        SELECT  ci.id,
                ci.cart_id,
                ci.product_id,
                ci.schedule_id,
                ci.quantity,
                c.created_at,
                c.is_checked_out
        FROM cart_items  AS ci
        JOIN carts       AS c  ON ci.cart_id = c.id
        WHERE c.user_id = @userId";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            await using var rd = await cmd.ExecuteReaderAsync();
            var ordId = rd.GetOrdinal("id");
            var ordCartId = rd.GetOrdinal("cart_id");
            var ordProductId = rd.GetOrdinal("product_id");
            var ordScheduleId = rd.GetOrdinal("schedule_id");
            var ordQuantity = rd.GetOrdinal("quantity");
            var ordCreatedAt = rd.GetOrdinal("created_at");
            var ordIsChecked = rd.GetOrdinal("is_checked_out");

            while (await rd.ReadAsync())
            {
                list.Add(new CartItemView
                {
                    Id = rd.GetInt32(ordId),
                    CartId = rd.GetInt32(ordCartId),
                    ProductId = rd.GetInt32(ordProductId),
                    ScheduleId = rd.IsDBNull(ordScheduleId) ? null : rd.GetInt32(ordScheduleId),
                    Quantity = rd.GetInt32(ordQuantity),
                    CreatedAt = rd.GetDateTime(ordCreatedAt),
                    IsCheckedOut = rd.GetBoolean(ordIsChecked)
                });
            }

            return list;
        }

    }
}
