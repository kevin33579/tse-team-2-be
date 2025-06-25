using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CartItemApi.Data;
using CartItemApi.Models;

namespace CartItemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]     // => /api/cartitems
    public class CartItemsController : ControllerBase
    {
        private readonly ICartItemRepository _repo;

        public CartItemsController(ICartItemRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Create a new cart item.
        /// </summary>
        /// <remarks>
        /// Sample request JSON:
        /// {
        ///   "cartId":     4,
        ///   "productId":  12,
        ///   "scheduleId": null,
        ///   "quantity":   3
        /// }
        /// </remarks>
        /// <returns>201 Created with the new resource location.</returns>
        [HttpPost]
        public async Task<ActionResult<CartItem>> CreateAsync([FromBody] CartItem item)
        {
            if (item is null) return BadRequest();
            if (item.Quantity <= 0) return BadRequest("Quantity must be positive.");

            // Persist to DB
            int newId = await _repo.CreateAsync(item);
            item.Id = newId;

            // Returns: 201 Created + Location header pointing to GET endpoint (if you add one later)
            return CreatedAtRoute(
                routeName: null,               // no GET route yet; pass null
                routeValues: new { id = newId },
                value: item);
        }

        [HttpGet("by-user/{userId:int}")]
        public async Task<ActionResult<IEnumerable<CartItemView>>> GetByUserAsync(int userId)
        {
            if (userId <= 0) return BadRequest("userId must be positive.");

            var items = await _repo.GetByUserIdAsync(userId);
            return Ok(items);   // 200 (empty list is still 200)
        }
    }
}
