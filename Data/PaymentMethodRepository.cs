using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using PaymentApi.Models;
using ProductApi.Exceptions;   // contains DatabaseException (reuse)

namespace PaymentApi.Data
{
    public interface IPaymentMethodRepository
    {
        Task<List<PaymentMethod>> GetAllAsync(CancellationToken ct = default);
        Task<PaymentMethod> CreatePaymentMethodAsync(PaymentMethod paymentMethod, CancellationToken ct = default);
        Task<PaymentMethod> UpdatePaymentMethodAsync(PaymentMethod paymentMethod, CancellationToken ct = default);
        Task DeletePaymentMethodAsync(uint id, CancellationToken ct = default);
        Task<PaymentMethod> GetPaymentMethodByIdAsync(uint id, CancellationToken ct = default);

    }

    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly string _connString;

        public PaymentMethodRepository(IConfiguration configuration)
        {
            _connString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }
        public async Task<List<PaymentMethod>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"SELECT id, name, imageUrl FROM paymentMethod";
            var methods = new List<PaymentMethod>();

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);

                // ──► Cast to MySqlDataReader to use GetUInt32("id") helpers
                await using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync(ct);

                while (await reader.ReadAsync(ct))
                {
                    methods.Add(new PaymentMethod
                    {
                        Id = reader.GetUInt32("id"),
                        Name = reader.GetString("name"),
                        ImageUrl = reader.GetString("imageUrl")
                    });
                }

                return methods;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT",
                    "Gagal mengambil data payment_method", ex);
            }
        }
        public async Task<PaymentMethod> CreatePaymentMethodAsync(PaymentMethod paymentMethod, CancellationToken ct = default)
        {
            const string sql = @"INSERT INTO paymentMethod (name, imageUrl) 
                                 VALUES (@name, @imageUrl);
                                 SELECT LAST_INSERT_ID();";

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", paymentMethod.Name);
                cmd.Parameters.AddWithValue("@imageUrl", paymentMethod.ImageUrl ?? string.Empty);

                // ──► Cast to MySqlDataReader to use GetUInt32("id") helpers
                var id = Convert.ToUInt32(await cmd.ExecuteScalarAsync(ct));

                paymentMethod.Id = id;
                return paymentMethod;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("INSERT",
                    "Gagal menambahkan data payment_method", ex);
            }
        }
        public async Task<PaymentMethod> UpdatePaymentMethodAsync(PaymentMethod paymentMethod, CancellationToken ct = default)
        {
            const string sql = @"UPDATE paymentMethod 
                                 SET name = @name, imageUrl = @imageUrl 
                                 WHERE id = @id;";

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", paymentMethod.Id);
                cmd.Parameters.AddWithValue("@name", paymentMethod.Name);
                cmd.Parameters.AddWithValue("@imageUrl", paymentMethod.ImageUrl ?? string.Empty);

                var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
                if (rowsAffected == 0)
                    throw new DatabaseException("UPDATE",
                        "Tidak ada baris yang diperbarui. Mungkin ID tidak ditemukan.");

                return paymentMethod;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("UPDATE",
                    "Gagal memperbarui data payment_method", ex);
            }
        }
        public async Task DeletePaymentMethodAsync(uint id, CancellationToken ct = default)
        {
            const string sql = @"DELETE FROM paymentMethod WHERE id = @id;";

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);

                var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
                if (rowsAffected == 0)
                    throw new DatabaseException("DELETE",
                        "Tidak ada baris yang dihapus. Mungkin ID tidak ditemukan.");
            }
            catch (Exception ex)
            {
                throw new DatabaseException("DELETE",
                    "Gagal menghapus data payment_method", ex);
            }
        }
        public async Task<PaymentMethod> GetPaymentMethodByIdAsync(uint id, CancellationToken ct = default)
        {
            const string sql = @"SELECT id, name, imageUrl FROM paymentMethod WHERE id = @id;";

            try
            {
                await using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync(ct);

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);

                // ──► Cast to MySqlDataReader to use GetUInt32("id") helpers
                await using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync(ct);

                if (await reader.ReadAsync(ct))
                {
                    return new PaymentMethod
                    {
                        Id = reader.GetUInt32("id"),
                        Name = reader.GetString("name"),
                        ImageUrl = reader.GetString("imageUrl")
                    };
                }
                else
                {
                    throw new DatabaseException("SELECT",
                        "Payment method tidak ditemukan dengan ID: " + id);
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT",
                    "Gagal mengambil data payment_method", ex);
            }
        }
    }

}