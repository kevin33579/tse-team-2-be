using Microsoft.AspNetCore.Mvc;     // Untuk Controller dan ActionResult
using ProductApi.Data;              // Untuk interface IProductRepository
using ProductApi.Models;            // Untuk model Product

namespace ProductApi.Controllers
{
    // [ApiController] - Attribute yang menandakan ini adalah API Controller
    // Memberikan fitur otomatis seperti model validation, error handling, dll
    [ApiController]
    // [Route] - Menentukan base URL untuk controller ini
    // [controller] akan diganti dengan nama controller tanpa "Controller" (Products)
    // Jadi URL base-nya adalah: /api/products
    [Route("api/products")]
    public class ProductsController : ControllerBase  // Inherit dari ControllerBase untuk API
    {
        // Field untuk menyimpan dependency yang akan di-inject
        private readonly IProductRepository _productRepository;  // Repository untuk akses data
        private readonly ILogger<ProductsController> _logger;    // Logger untuk mencatat aktivitas

        // Constructor - dipanggil saat controller dibuat
        // ASP.NET Core akan otomatis inject dependency yang dibutuhkan (Dependency Injection)
        public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger)
        {
            _productRepository = productRepository;  // Simpan repository instance
            _logger = logger;                       // Simpan logger instance
        }

        // =====================================
        // GET: api/products
        // =====================================
        // [HttpGet] - Menandakan ini adalah HTTP GET request
        // URL: GET /api/products
        [HttpGet]
        // async Task<ActionResult<List<Product>>> - Method async yang return list product
        // ActionResult memungkinkan kita return berbagai HTTP response (200, 404, 500, dll)
        public async Task<ActionResult<List<Product>>> GetProducts()
        {
            try  // Try-catch untuk menangani error
            {
                // Log informasi bahwa method ini dipanggil
                _logger.LogInformation("Mengambil semua produk");
                
                // Panggil repository untuk mengambil semua produk dari database
                // await = tunggu sampai operasi async selesai
                var products = await _productRepository.GetAllProductsAsync();
                
                // Return HTTP 200 OK dengan data products
                return Ok(products);
            }
            catch (Exception ex)  // Tangkap semua jenis exception
            {
                // Log error dengan detail exception
                _logger.LogError(ex, "Error saat mengambil produk");
                
                // Return HTTP 500 Internal Server Error
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        // =====================================
        // GET: api/products/5
        // =====================================
        // {id} dalam route menandakan ini adalah parameter dinamis
        // URL: GET /api/products/1, /api/products/2, dst
        [HttpGet("{id}")]
        // Parameter int id akan otomatis di-bind dari URL
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            try
            {
                // Panggil repository untuk mencari produk berdasarkan ID
                var product = await _productRepository.GetProductByIdAsync(id);
                
                // Cek apakah produk ditemukan
                if (product == null)
                {
                    // Return HTTP 404 Not Found jika produk tidak ada
                    return NotFound($"Produk dengan ID {id} tidak ditemukan");
                }
                
                // Return HTTP 200 OK dengan data produk
                return Ok(product);
            }
            catch (Exception ex)
            {
                // Log error dengan detail ID produk yang dicari
                _logger.LogError(ex, "Error saat mengambil produk dengan ID {ProductId}", id);
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        // =====================================
        // GET: api/products/price-range?minPrice=10&maxPrice=100
        // =====================================
        // URL dengan query parameters, contoh: /api/products/price-range?minPrice=100000&maxPrice=1000000
        
        // [FromQuery] - Parameter diambil dari query string UR

        // =====================================
        // POST: api/products
        // =====================================
        // [HttpPost] - Menandakan ini adalah HTTP POST request untuk membuat data baru
        [HttpPost]
        // [FromBody] - Data produk diambil dari request body (JSON)
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            try
            {
                // ModelState.IsValid - Cek apakah model memenuhi validasi yang didefinisikan
                // (Required, StringLength, Range, dll di model Product)
                if (!ModelState.IsValid)
                {
                    // Return HTTP 400 Bad Request dengan detail error validasi
                    return BadRequest(ModelState);
                }
                
                // Panggil repository untuk menyimpan produk baru ke database
                // Repository akan return ID produk yang baru dibuat
                var productId = await _productRepository.CreateProductAsync(product);
                
                // Set ID produk dengan ID yang baru dibuat
                product.id = productId;
                
                // CreatedAtAction - Return HTTP 201 Created
                // Parameter: nama action untuk GET by ID, route values, object yang dibuat
                // Header Location akan berisi URL untuk mengakses produk yang baru dibuat
                return CreatedAtAction(nameof(GetProduct), new { id = productId }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat membuat produk baru");
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        // =====================================
        // PUT: api/products/5
        // =====================================
        // [HttpPut] - Menandakan ini adalah HTTP PUT request untuk update data
        // URL: PUT /api/products/1, /api/products/2, dst
        [HttpPut("{id}")]
        // Parameter id dari URL dan product dari request body
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                // Validasi: ID di URL harus sama dengan ProductID di body
                // Ini untuk memastikan konsistensi data
                if (id != product.id)
                {
                    return BadRequest("ID produk tidak sesuai");
                }
                
                // Cek validasi model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                // Cek apakah produk yang akan diupdate ada di database
                var existingProduct = await _productRepository.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    // Return HTTP 404 jika produk tidak ditemukan
                    return NotFound($"Produk dengan ID {id} tidak ditemukan");
                }
                
                // Panggil repository untuk update produk
                var success = await _productRepository.UpdateProductAsync(product);
                
                if (success)
                {
                    // Return HTTP 204 No Content jika update berhasil
                    // No Content = operasi berhasil tapi tidak return data
                    return NoContent();
                }
                
                // Return HTTP 500 jika update gagal di database
                return StatusCode(500, "Gagal mengupdate produk");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate produk dengan ID {ProductId}", id);
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        // =====================================
        // DELETE: api/products/5
        // =====================================
        // [HttpDelete] - Menandakan ini adalah HTTP DELETE request untuk hapus data
        // URL: DELETE /api/products/1, /api/products/2, dst
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                // Cek dulu apakah produk yang akan dihapus ada di database
                var existingProduct = await _productRepository.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    // Return HTTP 404 jika produk tidak ditemukan
                    return NotFound($"Produk dengan ID {id} tidak ditemukan");
                }
                
                // Panggil repository untuk menghapus produk dari database
                var success = await _productRepository.DeleteProductAsync(id);
                
                if (success)
                {
                    // Return HTTP 204 No Content jika delete berhasil
                    return NoContent();
                }
                
                // Return HTTP 500 jika delete gagal di database
                return StatusCode(500, "Gagal menghapus produk");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus produk dengan ID {ProductId}", id);
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }
    }
}