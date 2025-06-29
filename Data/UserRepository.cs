using System.Data; // DataSet, DataRow, DataNulll
using MySql.Data.MySqlClient;
using UserApi.Models;

namespace UserApi.Data
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllProductsAsync();
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(RegisterRequest request);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
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
                    SELECT id, username, email,password,roleId createdDate 
                    FROM users ";

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
                                Id = reader.GetInt32("id"),
                                Username = reader.GetString("username"),
                                Email = reader.GetString("email"),
                                Password = reader.GetString("password"),
                                RoleID = reader.GetInt32("roleID"),
                                CreatedDate = reader.GetDateTime("createdDate")
                            };

                            // Tambahkan product ke list
                            users.Add(user);
                        }
                    }
                }
            }

            return users;

        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT * 
                    FROM users 
                    WHERE email = @email AND isActive = 1";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", email);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Email = reader.GetString(2),
                                Username = reader.IsDBNull(1) ? null : reader.GetString(3),
                                Password = reader.GetString(3),
                                CreatedDate = reader.GetDateTime(5),
                                LastLoginDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                IsActive = reader.GetBoolean(7)
                            };
                        }
                    }
                }
            }

            return null;
        }

        public async Task<bool> CreateUserAsync(RegisterRequest request)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
            INSERT INTO users (username, email, password, roleId, createdDate, isActive) 
            VALUES (@username, @email, @password, @roleId, @createdDate, @isActive)";

                using (var command = new MySqlCommand(query, connection))
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

                    command.Parameters.AddWithValue("@username", request.Username ?? "NewUser");
                    command.Parameters.AddWithValue("@email", request.Email);
                    command.Parameters.AddWithValue("@password", hashedPassword);
                    command.Parameters.AddWithValue("@roleId", 2); // default user role
                    command.Parameters.AddWithValue("@createdDate", DateTime.Now);
                    command.Parameters.AddWithValue("@isActive", true);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }


        public async Task<bool> UpdateLastLoginAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "UPDATE users SET lastLoginDate = @lastLoginDate WHERE id = @id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@lastLoginDate", DateTime.Now);
                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT COUNT(*) FROM users WHERE email = @email";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", email);

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }

    }


}