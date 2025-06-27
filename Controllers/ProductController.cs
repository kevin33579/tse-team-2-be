using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Models;
using Microsoft.Extensions.Logging;


namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<List<Product>>>> GetProducts()
        {
            try
            {
                _logger.LogInformation("Mengambil semua produk");
                var products = await _productRepository.GetAllProductsAsync();
                return Ok(ApiResult<List<Product>>.SuccessResult(products, "Produk berhasil diambil"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil produk");
                return StatusCode(500, ApiResult<List<Product>>.ErrorResult("Terjadi kesalahan server", 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<Product>>> GetProduct(int id)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(id);

                if (product == null)
                    return NotFound(ApiResult<Product>.ErrorResult($"Produk dengan ID {id} tidak ditemukan", 404));

                return Ok(ApiResult<Product>.SuccessResult(product, "Produk ditemukan"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil produk dengan ID {ProductId}", id);
                return StatusCode(500, ApiResult<Product>.ErrorResult("Terjadi kesalahan server", 500));
            }
        }

        [HttpGet("type/{productTypeId}")]
        public async Task<ActionResult<ApiResult<List<Product>>>> GetProductsByTypeId(int productTypeId)
        {
            try
            {
                var products = await _productRepository.GetProductsByTypeIdAsync(productTypeId);
                return Ok(ApiResult<List<Product>>.SuccessResult(products, "Produk berdasarkan tipe berhasil diambil"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil produk dengan ProductTypeId {ProductTypeId}", productTypeId);
                return StatusCode(500, ApiResult<List<Product>>.ErrorResult("Terjadi kesalahan server", 500));
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResult<List<Product>>>> SearchProducts(
            [FromQuery] string? searchTerm,
            [FromQuery] int? productTypeId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            try
            {
                var errors = new List<string>();
                if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                    errors.Add("Harga minimum tidak boleh lebih besar dari harga maksimum");
                if (minPrice < 0) errors.Add("Harga minimum tidak boleh negatif");
                if (maxPrice < 0) errors.Add("Harga maksimum tidak boleh negatif");

                if (errors.Any())
                    return BadRequest(ApiResult<List<Product>>.ErrorResult(errors, 400));

                _logger.LogInformation("SearchProducts dipanggil");
                var products = await _productRepository.SearchProductsAsync(searchTerm ?? "", productTypeId, minPrice, maxPrice);
                return Ok(ApiResult<List<Product>>.SuccessResult(products, "Hasil pencarian berhasil diambil"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Terjadi error saat mencari produk");
                return StatusCode(500, ApiResult<List<Product>>.ErrorResult("Terjadi kesalahan server", 500));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResult<Product>>> CreateProduct([FromBody] Product product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(ApiResult<Product>.ErrorResult(errors, 400));
                }

                var productId = await _productRepository.CreateProductAsync(product);
                product.id = productId;

                var result = ApiResult<Product>.SuccessResult(product, "Produk berhasil dibuat", 201);
                return CreatedAtAction(nameof(GetProduct), new { id = productId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat membuat produk baru");
                return StatusCode(500, ApiResult<Product>.ErrorResult("Terjadi kesalahan server", 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResult>> UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                if (id != product.id)
                    return BadRequest(ApiResult.ErrorResult("ID produk tidak sesuai", 400));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(ApiResult.ErrorResult(errors, 400));
                }

                var existingProduct = await _productRepository.GetProductByIdAsync(id);
                if (existingProduct == null)
                    return NotFound(ApiResult.ErrorResult($"Produk dengan ID {id} tidak ditemukan", 404));

                var success = await _productRepository.UpdateProductAsync(product);
                if (success)
                    return Ok(ApiResult.SuccessResult("Produk berhasil diupdate"));

                return StatusCode(500, ApiResult.ErrorResult("Gagal mengupdate produk", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate produk dengan ID {ProductId}", id);
                return StatusCode(500, ApiResult.ErrorResult("Terjadi kesalahan server", 500));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResult>> DeleteProduct(int id)
        {
            try
            {
                var existingProduct = await _productRepository.GetProductByIdAsync(id);
                if (existingProduct == null)
                    return NotFound(ApiResult.ErrorResult($"Produk dengan ID {id} tidak ditemukan", 404));

                var success = await _productRepository.DeleteProductAsync(id);
                if (success)
                    return Ok(ApiResult.SuccessResult("Produk berhasil dihapus"));

                return StatusCode(500, ApiResult.ErrorResult("Gagal menghapus produk", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus produk dengan ID {ProductId}", id);
                return StatusCode(500, ApiResult.ErrorResult("Terjadi kesalahan server", 500));
            }
        }
    }
}
