using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using CartApi.Models;          // make sure the Cart model lives here

namespace CartApi.Data
{
    /* ---------- Interface ---------- */
    public interface ICartRepository
    {
        /// <summary>
        /// Inserts a new Cart and returns the new Id.
        /// </summary>
        Task<int> CreateAsync(Cart cart);
    }

    /* ---------- Implementation ---------- */
    public class CartRepository : ICartRepository
    {
        private readonly string _connectionString;

        public CartRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        public async Task<int> CreateAsync(Cart cart)
        {
            if (cart == null) throw new ArgumentNullException(nameof(cart));

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Only user_id and is_checked_out need to be supplied; created_at defaults to NOW().
            const string sql = @"
                INSERT INTO carts (user_id, is_checked_out)
                VALUES (@userId, @isCheckedOut);";

            await using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@userId",       cart.UserId);
            cmd.Parameters.AddWithValue("@isCheckedOut", cart.IsCheckedOut);

            await cmd.ExecuteNonQueryAsync();

            // LastInsertedId contains the AUTO_INCREMENT value.
            return (int)cmd.LastInsertedId;
        }
    }
}
