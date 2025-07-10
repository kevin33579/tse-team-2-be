using System.Data; // DataSet, DataRow, DataNulll
using MySql.Data.MySqlClient; // MySqlConnection, MySqlCommand, MySqlDataAdapter
using ProductApi.Models; // Product
using ProductApi.Exceptions;
using ScheduleApi.Models; // Schedule
using ScheduleApi.Data; // IScheduleRepository


namespace ProductApi.Data
{
    public interface IProductRepository
    {
        Task<List<ProductDto>> GetAllProductsAsync();                                          // READ - Get all
        Task<ProductDto?> GetProductByIdAsync(int id);                                        // READ - Get by ID  
        Task<List<ProductDto>> GetProductsByTypeIdAsync(int productTypeId);                  // READ - Get by ProductType ID
        Task<List<Product>> SearchProductsAsync(string? searchTerm, int? productTypeId = null, decimal? minPrice = null, decimal? maxPrice = null);
        Task<List<ProductDto>> GetAllProductLimitsAsync();
        Task<int> CreateProductAsync(Product product);                                     // CREATE - Return new ID
        Task<bool> UpdateProductAsync(Product product);
        Task<List<Schedule>> GetSchedulesByProductIdAsync(int productId);
        // UPDATE - Return success status
        Task<bool> DeleteProductAsync(int id);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            var products = new List<ProductDto>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string queryString = @"
        SELECT  p.id,
                p.name,
                p.price,
                p.stock,
                p.description,
                p.imageUrl,
                p.productTypeId,
                pt.name AS productTypeName          -- ① alias persis!
        FROM    product p
        LEFT JOIN producttype pt                -- ② nama tabel sesuai definisi
               ON pt.id = p.productTypeId
        ORDER BY p.id ;";
                    using (var command = new MySqlCommand(queryString, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var product = new ProductDto
                            {
                                id = reader.GetInt32("id"),
                                name = reader.GetString("name"),
                                price = reader.GetDecimal("price"),
                                stock = reader.GetInt32("stock"),
                                description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                                productTypeId = reader.IsDBNull("productTypeId") ? null : reader.GetInt32("productTypeId"),
                                imageUrl = reader.IsDBNull("imageUrl") ? null : reader.GetString("imageUrl"),
                                productTypeName = reader.GetString("productTypeName")
                            };
                            products.Add(product);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT", "Gagal mengambil data produk", ex);
            }

            return products;
        }

        public async Task<List<ProductDto>> GetAllProductLimitsAsync()
        {
            var products = new List<ProductDto>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string queryString = @"
        SELECT  p.id,
                p.name,
                p.price,
                p.stock,
                p.description,
                p.imageUrl,
                p.productTypeId,
                pt.name AS productTypeName          -- ① alias persis!
        FROM    product p
        LEFT JOIN producttype pt                -- ② nama tabel sesuai definisi
               ON pt.id = p.productTypeId
        ORDER BY p.name LIMIT 6;";
                    using (var command = new MySqlCommand(queryString, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var product = new ProductDto
                            {
                                id = reader.GetInt32("id"),
                                name = reader.GetString("name"),
                                price = reader.GetDecimal("price"),
                                stock = reader.GetInt32("stock"),
                                description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                                productTypeId = reader.IsDBNull("productTypeId") ? null : reader.GetInt32("productTypeId"),
                                imageUrl = reader.IsDBNull("imageUrl") ? null : reader.GetString("imageUrl"),
                                productTypeName = reader.GetString("productTypeName")
                            };
                            products.Add(product);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT", "Gagal mengambil data produk", ex);
            }

            return products;
        }
        public async Task<List<Schedule>> GetSchedulesByProductIdAsync(int productId)
        {
            var schedules = new List<Schedule>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
        SELECT s.id, s.time
        FROM schedule s
        JOIN product_schedule ps ON s.id = ps.schedule_id
        WHERE ps.product_id = @productId;
    ";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@productId", productId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                schedules.Add(new Schedule
                {
                    Id = reader.GetInt32("Id"),
                    Time = reader.GetDateTime("Time")
                });
            }

            return schedules;
        }


        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string queryString = @"
               SELECT  p.id,
                p.name,
                p.price,
                p.stock,
                p.description,
                p.imageUrl,
                p.productTypeId,
                pt.name AS productTypeName          -- ① alias persis!
        FROM    product p
        LEFT JOIN producttype pt                -- ② nama tabel sesuai definisi
               ON pt.id = p.productTypeId
                WHERE p.id = @id";

                    using (var command = new MySqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new ProductDto
                                {
                                    id = reader.GetInt32("id"),
                                    name = reader.GetString("name"),
                                    price = reader.GetDecimal("price"),
                                    stock = reader.GetInt32("stock"),
                                    description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                                    productTypeId = reader.IsDBNull("productTypeId") ? null : reader.GetInt32("productTypeId"),
                                    imageUrl = reader.IsDBNull("imageUrl") ? null : reader.GetString("imageUrl"),
                                    productTypeName = reader.GetString("productTypeName")
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT", $"Gagal mengambil produk dengan ID: {id}", ex);
            }
        }


        public async Task<List<ProductDto>> GetProductsByTypeIdAsync(int productTypeId)
        {
            var products = new List<ProductDto>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string queryString = @"
                SELECT  p.id,
                p.name,
                p.price,
                p.stock,
                p.description,
                p.imageUrl,
                p.productTypeId,
                pt.name AS productTypeName          -- ① alias persis!
        FROM    product p
        LEFT JOIN producttype pt                -- ② nama tabel sesuai definisi
               ON pt.id = p.productTypeId
                WHERE pt.id = @productTypeId"; ;

                    using (var command = new MySqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@productTypeId", productTypeId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var product = new ProductDto
                                {
                                    id = reader.GetInt32("id"),
                                    name = reader.GetString("name"),
                                    price = reader.GetDecimal("price"),
                                    stock = reader.GetInt32("stock"),
                                    description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                                    productTypeId = reader.IsDBNull("productTypeId") ? null : reader.GetInt32("productTypeId"),
                                    imageUrl = reader.IsDBNull("imageUrl") ? null : reader.GetString("imageUrl"),
                                };

                                products.Add(product);
                            }
                        }
                    }
                }

                return products;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT", $"Gagal mengambil produk berdasarkan productTypeId: {productTypeId}", ex);
            }
        }


        public async Task<List<Product>> SearchProductsAsync(string? searchTerm, int? productTypeId = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var products = new List<Product>();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
            SELECT * 
            FROM product
            WHERE 1=1";

                var parameters = new List<MySqlParameter>();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    sql += " AND (name LIKE @searchTerm OR description LIKE @searchTerm)";
                    parameters.Add(new MySqlParameter("@searchTerm", $"%{searchTerm}%"));
                }

                if (productTypeId.HasValue)
                {
                    sql += " AND productTypeId = @productTypeId";
                    parameters.Add(new MySqlParameter("@productTypeId", productTypeId.Value));
                }

                if (minPrice.HasValue)
                {
                    sql += " AND price >= @minPrice";
                    parameters.Add(new MySqlParameter("@minPrice", minPrice.Value));
                }

                if (maxPrice.HasValue)
                {
                    sql += " AND price <= @maxPrice";
                    parameters.Add(new MySqlParameter("@maxPrice", maxPrice.Value));
                }

                sql += " ORDER BY name";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddRange(parameters.ToArray());

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var product = new Product
                    {
                        id = reader.GetInt32("id"),
                        name = reader.GetString("name"),
                        price = reader.GetDecimal("price"),
                        stock = reader.GetInt32("stock"),
                        description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                        productTypeId = reader.IsDBNull("productTypeId") ? null : reader.GetInt32("productTypeId"),
                        imageUrl = reader.IsDBNull("imageUrl") ? null : reader.GetString("imageUrl")
                    };

                    products.Add(product);
                }

                return products;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("SELECT", "Gagal melakukan pencarian produk", ex);
            }
        }



        public async Task<int> CreateProductAsync(Product product)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                string queryString = @"
            INSERT INTO product (name, price, stock, description, productTypeId, imageUrl)
            VALUES (@name, @price, @stock, @description, @productTypeId, @imageUrl);
            SELECT LAST_INSERT_ID();";

                using var command = new MySqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@name", product.name);
                command.Parameters.AddWithValue("@price", product.price);
                command.Parameters.AddWithValue("@stock", product.stock);
                command.Parameters.AddWithValue("@description", product.description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@productTypeId", product.productTypeId);
                command.Parameters.AddWithValue("@imageUrl", product.imageUrl ?? (object)DBNull.Value);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("INSERT", "Gagal membuat produk", ex);
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string queryString = @"
                UPDATE product 
                SET name = @name, 
                    price = @price, 
                    stock = @stock, 
                    description = @description,
                    productTypeId = @productTypeId,
                    imageUrl = @imageUrl
                WHERE id = @id";

                    using (var command = new MySqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@id", product.id);
                        command.Parameters.AddWithValue("@name", product.name);
                        command.Parameters.AddWithValue("@price", product.price);
                        command.Parameters.AddWithValue("@stock", product.stock);
                        command.Parameters.AddWithValue("@description", product.description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@productTypeId", product.productTypeId);
                        command.Parameters.AddWithValue("@imageUrl", product.imageUrl ?? (object)DBNull.Value);

                        var rowsAffected = await command.ExecuteNonQueryAsync();

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseException("UPDATE", $"Gagal mengupdate produk dengan ID: {product.id}", ex);
            }
        }

        /// <summary>
        /// Menghapus produk dari database berdasarkan ID
        /// Method ini menunjukkan penggunaan ExecuteNonQuery untuk operasi DELETE
        /// </summary>
        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                string queryString = "DELETE FROM product WHERE id = @id";

                using var command = new MySqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@id", id);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("DELETE", $"Gagal menghapus produk dengan ID {id}", ex);
            }
        }

    }
}