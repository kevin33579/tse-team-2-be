using System.Data; // DataSet, DataRow, DataNulll
using MySql.Data.MySqlClient; // MySqlConnection, MySqlCommand, MySqlDataAdapter
using ProductApi.Models; // Product

namespace ProductApi.Data
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllProductsAsync();                                          // READ - Get all
        Task<Product?> GetProductByIdAsync(int id);                                        // READ - Get by ID  
        Task<int> CreateProductAsync(Product product);                                     // CREATE - Return new ID
        Task<bool> UpdateProductAsync(Product product);                                    // UPDATE - Return success status
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

        // public async Task<List<Product>> GetAllProductsAsync()
        // {
        //     var products = new List<Product>();

        //     using (var connection = new MySqlConnection(_connectionString))
        //     {
        //         await connection.OpenAsync();
        //         string queryString = @"
        //             SELECT * 
        //             FROM product 
        //             ORDER BY name";
        //         using (var command = new MySqlCommand(queryString, connection))
        //         {
        //             using (var reader = await command.ExecuteReaderAsync())
        //             {
        //                 while (await reader.ReadAsync())
        //                 {
        //                     var product = new Product
        //                     {
        //                         id = reader.GetInt32("id"),
        //                         name = reader.GetString("name"),
        //                         price = reader.GetDecimal("price"),
        //                         stock = reader.GetInt32("stock"),
        //                         description = reader.IsDBNull("description") ? null : reader.GetString("description"),
        //                         productTypeId = reader.IsDBNull("productTypeId") ? null : reader.GetInt32("productTypeId")

        //                     };

        //                     // Tambahkan product ke list
        //                     products.Add(product);
        //                 }
        //             }
        //         }
        //     }

        //     return products;
        // }
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string queryString = @"
        SELECT * 
        FROM product 
        ORDER BY name";
                    using (var command = new MySqlCommand(queryString, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var product = new Product
                                {
                                    id = reader.GetInt32("id"),
                                    name = reader.GetString("name"),
                                    price = reader.GetDecimal("price"),
                                    stock = reader.GetInt32("stock"),
                                    description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                                    productTypeId = reader.IsDBNull("productTypeId") ? null : reader.GetInt32("productTypeId")
                                };

                                products.Add(product);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR GetAllProductsAsync: " + ex.Message);
                throw;
            }

            return products;
        }
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string queryString = @"
                    SELECT *
                    FROM product 
                    WHERE id = @id";

                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Product
                            {
                                id = reader.GetInt32("id"),
                                name = reader.GetString("name"),
                                price = reader.GetDecimal("price"),
                                stock = reader.GetInt32("stock"),
                                description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                                productTypeId = reader.IsDBNull("productTypeId") ? null : reader.GetInt32("productTypeId")

                            };
                        }
                    }
                }
            }

            
            return null;
        }
        public async Task<int> CreateProductAsync(Product product)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Query INSERT dengan LAST_INSERT_ID() untuk mendapatkan ID yang baru dibuat
                // LAST_INSERT_ID() return ID terakhir yang di-insert dalam connection saat ini
                string queryString = @"
                    INSERT INTO product (name, price, stock, description, productTypeId)
                    VALUES (@name, @price, @stock, @description, @productTypeId);
                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@name", product.name);
                    command.Parameters.AddWithValue("@price", product.price);
                    command.Parameters.AddWithValue("@stock", product.stock);
                    command.Parameters.AddWithValue("@description", product.description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@productTypeId", product.productTypeId);

                    var result = await command.ExecuteScalarAsync();

                    return Convert.ToInt32(result);
                }
            }
        }
        public async Task<bool> UpdateProductAsync(Product product)
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
                        productTypeId = @productTypeId
                    WHERE id = @id";

                using (var command = new MySqlCommand(queryString, connection))
                {
                    // Parameter untuk UPDATE (termasuk ProductID untuk WHERE clause)
                    command.Parameters.AddWithValue("@id", product.id);
                    command.Parameters.AddWithValue("@name", product.name);
                    command.Parameters.AddWithValue("@price", product.price);
                    command.Parameters.AddWithValue("@stock", product.stock);
                    command.Parameters.AddWithValue("@description", product.description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@productTypeId", product.productTypeId);

                    // ExecuteNonQuery = untuk operasi yang tidak return data (INSERT, UPDATE, DELETE)
                    // Return int = jumlah rows yang affected
                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    // Return true jika ada row yang ter-update (rowsAffected > 0)
                    // Return false jika tidak ada row yang ter-update (kemungkinan ID tidak ditemukan)
                    return rowsAffected > 0;
                }
            }
        }

        /// <summary>
        /// Menghapus produk dari database berdasarkan ID
        /// Method ini menunjukkan penggunaan ExecuteNonQuery untuk operasi DELETE
        /// </summary>
        public async Task<bool> DeleteProductAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Query DELETE sederhana dengan WHERE clause
                string queryString = "DELETE FROM product WHERE id = @id";

                using (var command = new MySqlCommand(queryString, connection))
                {
                    // Parameter untuk WHERE clause
                    command.Parameters.AddWithValue("@id", id);

                    // ExecuteNonQuery untuk operasi DELETE
                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    // Return true jika ada row yang terhapus
                    // Return false jika tidak ada row yang terhapus (ID tidak ditemukan)
                    return rowsAffected > 0;
                }
            }
        }
    }
}