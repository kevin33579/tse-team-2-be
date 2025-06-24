using System.Data; // DataSet, DataRow, DataNulll
using MySql.Data.MySqlClient;
using UserApi.Models;

namespace UserApi.Data
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllProductsAsync();
    }

    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        public UserRepository(IConfiguration configuration)
        {
            // Ambil connection string dari appsettings.json
            // ?? throw new ArgumentNullException = jika null, lempar exception
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }
        public async Task<List<User>> GetAllProductsAsync()
        {
            var users = new List<User>();

            // using statement = otomatis dispose connection setelah selesai
            // MySqlConnection = membuka koneksi ke MySQL Server
            using (var connection = new MySqlConnection(_connectionString))
            {
                // OpenAsync = membuka koneksi secara asynchronous
                await connection.OpenAsync();

                // Query SQL untuk mengambil semua produk
                // @ untuk multiline string, lebih mudah dibaca
                string queryString = @"
                    SELECT Id, Username, Email, CreatedDate 
                    FROM Users ";

                // MySqlCommand = object untuk menjalankan SQL command
                using (var command = new MySqlCommand(queryString, connection))
                {
                    // ExecuteReaderAsync = menjalankan SELECT query dan return MySqlDataReader
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // ReadAsync = membaca row berikutnya, return true jika ada data
                        while (await reader.ReadAsync())
                        {
                            var user = new User
                            {
                                Id = reader.GetInt32("Id"),
                                Username = reader.GetString("UserName"),
                                Email = reader.GetString("Email"),
                                Password = reader.GetString("Password"),
                                RoleID = reader.GetInt32("RoleID"),
                                CreatedDate = reader.GetDateTime("CreatedDate")
                            };

                            // Tambahkan product ke list
                            users.Add(user);
                        }
                    }
                }
            }

            return users;

        }

    }


}