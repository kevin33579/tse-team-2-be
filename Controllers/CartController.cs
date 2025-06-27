using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartApi.Data;
using CartApi.Models;

namespace CartApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;

        public CartController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        // GET: api/cart/user/1
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Cart>>> GetCartsByUser(uint userId)
        {
            var carts = await _cartRepository.GetCartsByUserAsync(userId);

            if (carts == null || carts.Count == 0)
            {
                return NotFound($"No cart items found for user with ID {userId}.");
            }

            return Ok(carts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cart>> GetCartById(uint id)
        {
            var list = await _cartRepository.GetCartsByUserAsync( /* hack:  we only have GetByUser  */
                userId: 0,                                          /*        so this just demo-stub */
                HttpContext.RequestAborted);

            var cart = list.Find(c => c.Id == id);
            if (cart == null)
                return NotFound();

            return Ok(cart);
        }

        [HttpPost]
        public async Task<ActionResult<Cart>> CreateCart([FromBody] Cart cart)
        {
            // Basic model validation
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Insert row and fetch new auto-increment id
            uint newId = await _cartRepository.CreateCartAsync(cart, HttpContext.RequestAborted);
            cart.Id = newId;

            // Return 201 Created with Location header
            return CreatedAtAction(
                nameof(GetCartById),
                new { id = newId },
                cart);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(uint id)
        {
            bool removed = await _cartRepository.DeleteCartAsync(id, HttpContext.RequestAborted);

            return removed ? NoContent()                          // 204
                           : NotFound($"Cart item {id} not found"); // 404
        }
    }
}
