using Microsoft.AspNetCore.Mvc;
using ProductTypeApi.Data;
using ProductTypeApi.Models;

namespace ProductTypeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // → api/ProductTypes
    public class ProductTypesController : ControllerBase
    {
        private readonly IProductTypeRepository _repo;

        public ProductTypesController(IProductTypeRepository repo)
        {
            _repo = repo;
        }

        /*───────────────────────────────────────────────────────────*
         * GET: api/ProductTypes
         *───────────────────────────────────────────────────────────*/
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductType>>> GetAll()
        {
            var items = await _repo.GetAllProductTypesAsync();
            return Ok(items);
        }

        /*───────────────────────────────────────────────────────────*
         * GET: api/ProductTypes/5
         *    (requires a repo method that fetches by Id; see note)
         *───────────────────────────────────────────────────────────*/
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductType>> GetById(int id)
        {
            // If you add GetProductTypeByIdAsync to the repo, use that here.
            var item = (await _repo.GetAllProductTypesAsync())
                       .FirstOrDefault(p => p.Id == id);

            if (item is null) return NotFound();
            return Ok(item);
        }

        /*───────────────────────────────────────────────────────────*
         * POST: api/ProductTypes
         *───────────────────────────────────────────────────────────*/
        [HttpPost]
        public async Task<ActionResult<ProductType>> Create(ProductType dto)
        {
            // Basic validation: name required
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");

            var newId       = await _repo.CreateProductTypeAsync(dto);
            dto.Id          = newId;         // echo new key


            // Returns 201 + Location header
            return CreatedAtAction(nameof(GetById),
                                   new { id = newId },
                                   dto);
        }

        /*───────────────────────────────────────────────────────────*
         * PUT: api/ProductTypes/5
         *───────────────────────────────────────────────────────────*/
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, ProductType dto)
        {
            if (id != dto.Id)
                return BadRequest("Id in URL and body do not match.");

            var success = await _repo.UpdateProductTypeAsync(dto);
            return success ? NoContent() : NotFound();
        }

        /*───────────────────────────────────────────────────────────*
         * DELETE: api/ProductTypes/5
         *───────────────────────────────────────────────────────────*/
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _repo.DeleteProductTypeAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
