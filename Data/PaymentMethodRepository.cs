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
    }
}
