using System.Data;
using MySql.Data.MySqlClient;
using ProductTypeApi.Models;

namespace ProductTypeApi.Data
{
    /*───────────────────────────────────────────────────────────────*
     * 1. Interface
     *───────────────────────────────────────────────────────────────*/
    public interface IProductTypeRepository
    {
        Task<List<ProductType>> GetAllProductTypesAsync();
        Task<int> CreateProductTypeAsync(ProductType productType);   // returns new Id

        Task<bool> UpdateProductTypeAsync(ProductType productType);   // true = success
        Task<bool> DeleteProductTypeAsync(int id);                    // true = success
    }

    /*───────────────────────────────────────────────────────────────*
     * 2. Implementation
     *───────────────────────────────────────────────────────────────*/
    public class ProductTypeRepository : IProductTypeRepository
    {
        private readonly string _connectionString;

        public ProductTypeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        /*──────────────────────────────────────────────*
         * READ – all rows
         *──────────────────────────────────────────────*/
        public async Task<List<ProductType>> GetAllProductTypesAsync()
        {
            var productTypes = new List<ProductType>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"SELECT *
                                 FROM ProductType;";

            await using var cmd = new MySqlCommand(sql, connection);
            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                productTypes.Add(new ProductType
                {
                    Id = rdr.GetInt32("id"),
                    Name = rdr.GetString("name"),
                    Description = rdr.IsDBNull("description") ? null : rdr.GetString("description"),
                    ImageUrl = rdr.GetString("imageUrl"),
                });
            }
            return productTypes;
        }

        /*──────────────────────────────────────────────*
         * CREATE – insert row, return new Id
         *──────────────────────────────────────────────*/
        public async Task<int> CreateProductTypeAsync(ProductType productType)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                INSERT INTO ProductType (name, description)
                VALUES (@name, @description);
                SELECT LAST_INSERT_ID();";

            await using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@name", productType.Name);
            cmd.Parameters.AddWithValue("@description", (object?)productType.Description ?? DBNull.Value);

            var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return newId;                               // you can also set productType.Id = newId;
        }

        /*──────────────────────────────────────────────*
         * UPDATE – change name/description by Id
         *──────────────────────────────────────────────*/
        public async Task<bool> UpdateProductTypeAsync(ProductType productType)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                UPDATE ProductType
                SET name        = @name,
                    description = @description
                WHERE id = @id;";

            await using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@name", productType.Name);
            cmd.Parameters.AddWithValue("@description", (object?)productType.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", productType.Id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;                            // true if something was updated
        }

        /*──────────────────────────────────────────────*
         * DELETE – remove by Id
         *──────────────────────────────────────────────*/
        public async Task<bool> DeleteProductTypeAsync(int id)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"DELETE FROM ProductType WHERE id = @id;";

            await using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;                            // true if row(s) deleted
        }
    }
}
