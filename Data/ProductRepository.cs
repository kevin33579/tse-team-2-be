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

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT * 
                    FROM Products 
                    ORDER BY Name";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var product = new Product
                            {
                                ProductId = reader.GetInt32("ProductId"),
                                Name = reader.GetString("Name"),
                                Price = reader.GetDecimal("Price"),
                                Stock = reader.GetInt32("Stock"),
                                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                                ProductTypeID = reader.GetInt32("ProductTypeID")
                            };
                            
                            // Tambahkan product ke list
                            products.Add(product);
                        }
                    }
                }
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
                    FROM Products 
                    WHERE ProductID = @ProductID";
                
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", id);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Product
                            {
                                ProductId = reader.GetInt32("ProductId"),
                                Name = reader.GetString("Name"),
                                Price = reader.GetDecimal("Price"),
                                Stock = reader.GetInt32("Stock"),
                                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                                ProductTypeID = reader.GetInt32("ProductTypeID")
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
                    INSERT INTO Products (Name, Price, Stock, Description, ProductTypeID)
                    VALUES (@Name, @Price, @Stock, @Description, @ProductTypeID);
                    SELECT LAST_INSERT_ID();";
                
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@Stock", product.Stock);
                    command.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ProductTypeId", product.ProductTypeID);
                    
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
                    UPDATE Products 
                    SET Name = @Name, 
                        Price = @Price, 
                        Stock = @Stock, 
                        Description = @Description
                        ProductTypeID = @ProductTypeID
                    WHERE ProductId = @ProductId";
                
                using (var command = new MySqlCommand(queryString, connection))
                {
                    // Parameter untuk UPDATE (termasuk ProductID untuk WHERE clause)
                    command.Parameters.AddWithValue("@ProductId", product.ProductId);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@price", product.Price);
                    command.Parameters.AddWithValue("@Stock", product.Stock);
                    command.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ProductTypeID", product.ProductTypeID);
                    
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
                string queryString = "DELETE FROM Products WHERE ProductId = @ProductId";
                
                using (var command = new MySqlCommand(queryString, connection))
                {
                    // Parameter untuk WHERE clause
                    command.Parameters.AddWithValue("@ProductId", id);
                    
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