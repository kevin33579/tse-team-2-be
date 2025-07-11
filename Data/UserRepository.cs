using System.Data; // DataSet, DataRow, DataNulll
using MySql.Data.MySqlClient;
using UserApi.Models;
using UserApi.Services;

namespace UserApi.Data
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllProductsAsync();
        Task<User?> GetUserByEmailAsync(string email);

        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> DeleteUser(int id); // Delete user by ID
        Task<bool> CreateUserAsync(RegisterRequest request);
        Task<bool> UpdateLastLoginAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<(User user, string roleName)?> GetUserWithRoleByEmailAsync(string email);
        Task<List<User>> SearchUsersAsync(string? searchTerm);

        Task<bool> VerifyUserEmailAsync(int id);
        Task<bool> VerifyEmailAsync(string token); // Verify by token
        Task<string> UpdateEmailVerificationTokenAsync(int id);
        Task<string> UpdatePasswordResetTokenAsync(int id);
        Task<User?> GetUserByPasswordResetTokenAsync(string token);
        Task<bool> UpdatePasswordAndClearResetTokenAsync(int id, string newHashedPassword);

        Task<bool> DeactivateUserAsync(int userId);

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

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            const string sql = @"UPDATE users SET isActive = 0 WHERE id = @id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            int affectedRows = await cmd.ExecuteNonQueryAsync();

            return affectedRows > 0;
        }





        // ─────────────────────────────────────────────────────────────
        // Get every user and include the role name   (JOIN roles)
        // ─────────────────────────────────────────────────────────────
        public async Task<List<User>> GetAllUsersAsync()
        {
            const string sql = @"
        SELECT  u.id,
                u.username,
                u.email,
                u.password,
                u.roleId,
                r.name       AS roleName,
                u.createdDate,
                u.lastLoginDate,
                u.isActive
        FROM    users u
        JOIN    roles r ON r.id = u.roleId
         WHERE   u.isActive = 1
        ";

            var list = new List<User>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand(sql, conn);
            await using var rdr = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new User
                {
                    Id = rdr.GetInt32("id"),
                    Username = rdr.IsDBNull("username") ? null : rdr.GetString("username"),
                    Email = rdr.GetString("email"),
                    Password = rdr.GetString("password"),
                    RoleID = rdr.GetInt32("roleId"),
                    RoleName = rdr.GetString("roleName"),   // ← added
                    CreatedDate = rdr.GetDateTime("createdDate"),
                    LastLoginDate = rdr.IsDBNull("lastLoginDate") ? null : rdr.GetDateTime("lastLoginDate"),
                    IsActive = rdr.GetBoolean("isActive")
                });
            }
            return list;
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
                                Username = reader.IsDBNull(1) ? null : reader.GetString(1),
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
        public async Task<User?> DeleteUser(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string selectQuery = @"SELECT id, username, email, password, roleId, createdDate
                               FROM users WHERE id = @id";
                User? userToDelete = null;

                using (var selectCommand = new MySqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@id", id);

                    using (var reader = await selectCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userToDelete = new User
                            {
                                Id = reader.GetInt32("id"),
                                Username = reader.GetString("username"),
                                Email = reader.GetString("email"),
                                Password = reader.GetString("password"),
                                RoleID = reader.GetInt32("roleId"),
                                CreatedDate = reader.GetDateTime("createdDate")
                            };
                        }
                    }
                }

                if (userToDelete == null)
                {
                    return null; // tidak ada user yang cocok
                }

                string deleteQuery = @"DELETE FROM users WHERE id = @id";
                using (var deleteCommand = new MySqlCommand(deleteQuery, connection))
                {
                    deleteCommand.Parameters.AddWithValue("@id", id);

                    await deleteCommand.ExecuteNonQueryAsync();
                }

                return userToDelete;
            }
        }


        public async Task<User?> GetUserByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT * 
                    FROM users 
                    WHERE id = @id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Email = reader.GetString(2),
                                Username = reader.IsDBNull(1) ? null : reader.GetString(1),
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

        // inside UserRepository
        public async Task<(User user, string roleName)?> GetUserWithRoleByEmailAsync(string email)
        {
            const string sql = @"
        SELECT  u.id, u.username, u.email, u.password, u.createdDate,
                u.lastLoginDate, u.isActive, u.roleId,
                r.name AS roleName
        FROM    users u
        JOIN    roles r ON r.id = u.roleId        -- 🔗 join to roles
        WHERE   u.email = @email
          AND   u.isActive = 1
        LIMIT 1;";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", email);

            await using var rdr = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
            {
                var user = new User
                {
                    Id = rdr.GetInt32("id"),
                    Username = rdr.IsDBNull("username") ? null : rdr.GetString("username"),
                    Email = rdr.GetString("email"),
                    Password = rdr.GetString("password"),
                    RoleID = rdr.GetInt32("roleId"),
                    CreatedDate = rdr.GetDateTime("createdDate"),
                    LastLoginDate = rdr.IsDBNull("lastLoginDate") ? null : rdr.GetDateTime("lastLoginDate"),
                    IsActive = rdr.GetBoolean("isActive")
                };

                string roleName = rdr.GetString("roleName");
                return (user, roleName);
            }

            return null;
        }

        public async Task<List<User>> SearchUsersAsync(string? searchTerm)
        {
            var users = new List<User>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string query = @"
        SELECT u.id, u.username, u.email, u.password, u.roleId,
               r.name AS roleName,
               u.createdDate, u.lastLoginDate, u.isActive
        FROM users u
        JOIN roles r ON u.roleId = r.id
        WHERE (@searchTerm IS NULL OR u.username LIKE @searchPattern OR u.email LIKE @searchPattern)
        ORDER BY u.createdDate DESC;
    ";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@searchTerm", string.IsNullOrWhiteSpace(searchTerm) ? DBNull.Value : searchTerm);
            command.Parameters.AddWithValue("@searchPattern", $"%{searchTerm}%");

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32("id"),
                    Username = reader.IsDBNull("username") ? null : reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Password = reader.GetString("password"),
                    RoleID = reader.GetInt32("roleId"),
                    RoleName = reader.GetString("roleName"),
                    CreatedDate = reader.GetDateTime("createdDate"),
                    LastLoginDate = reader.IsDBNull("lastLoginDate") ? null : reader.GetDateTime("lastLoginDate"),
                    IsActive = reader.GetBoolean("isActive")
                });
            }

            return users;
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

        public async Task<bool> VerifyUserEmailAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"UPDATE users 
                               SET isEmailVerified = TRUE, 
                                   emailVerificationToken = NULL, 
                                   emailTokenCreatedAt = NULL 
                               WHERE id = @id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check token validity and update user
                string query = @"UPDATE users 
                               SET isEmailVerified = 1, 
                                   emailVerificationToken = NULL, 
                                   emailTokenCreatedAt = NULL 
                               WHERE emailVerificationToken = @token 
                               AND emailTokenCreatedAt > DATE_SUB(NOW(), INTERVAL 24 HOUR)
                               AND isEmailVerified = 0";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@token", token);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<string> UpdateEmailVerificationTokenAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string newToken = Guid.NewGuid().ToString();
                string query = @"
            UPDATE users 
            SET emailVerificationToken = @token, 
                emailTokenCreatedAt = @createdDate 
            WHERE id = @id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@token", newToken);
                    command.Parameters.AddWithValue("@createdDate", DateTime.Now);
                    command.Parameters.AddWithValue("@id", id);

                    await command.ExecuteNonQueryAsync();
                }

                return newToken;
            }
        }

        /*************  ✨ Windsurf Command ⭐  *************/
        /// <summary>
        /// Update the password reset token for the given user ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>The new password reset token.</returns>
        /*******  bcf19c9a-46a5-4f39-a899-2ae27efc03ea  *******/
        public async Task<string> UpdatePasswordResetTokenAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string newToken = Guid.NewGuid().ToString();
                string query = @"
            UPDATE users 
            SET passwordResetToken = @token, 
                passwordResetTokenCreatedAt = @createdDate
            WHERE id = @id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@token", newToken);
                    command.Parameters.AddWithValue("@createdDate", DateTime.Now);
                    command.Parameters.AddWithValue("@id", id);

                    await command.ExecuteNonQueryAsync();
                }

                return newToken;
            }
        }

        public async Task<User?> GetUserByPasswordResetTokenAsync(string token)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
            SELECT * 
            FROM users 
            WHERE PasswordResetToken = @token 
              AND passwordResetTokenCreatedAt > DATE_SUB(NOW(), INTERVAL 1 HOUR)";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@token", token);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = reader.GetInt32("id"),
                                Username = reader.GetString("username"),
                                Email = reader.GetString("email"),
                                Password = reader.GetString("password"),
                                RoleID = reader.GetInt32("roleID"),
                                CreatedDate = reader.GetDateTime("createdDate"),
                                LastLoginDate = reader.IsDBNull("lastLoginDate") ? null : reader.GetDateTime("lastLoginDate"),
                                IsActive = reader.GetBoolean("isActive"),
                                IsEmailVerified = reader.GetBoolean("isEmailVerified"),
                                EmailVerificationToken = reader.IsDBNull("emailVerificationToken") ? null : reader.GetString("emailVerificationToken"),
                                EmailTokenCreatedAt = reader.IsDBNull("emailTokenCreatedAt") ? null : reader.GetDateTime("emailTokenCreatedAt"),
                                PasswordResetToken = reader.IsDBNull("passwordResetToken") ? null : reader.GetString("passwordResetToken"),
                                PasswordResetTokenCreatedAt = reader.IsDBNull("passwordResetTokenCreatedAt") ? null : reader.GetDateTime("passwordResetTokenCreatedAt"),
                            };
                        }
                    }
                }
            }

            return null;
        }

        public async Task<bool> UpdatePasswordAndClearResetTokenAsync(int id, string newHashedPassword)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
            UPDATE users 
            SET password = @Password,
                passwordResetToken = NULL,
                passwordResetTokenCreatedAt = NULL
            WHERE id = @id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@password", newHashedPassword);
                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }


    }
}