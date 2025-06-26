using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CartApi.Data;
using CartApi.Models;

namespace CartApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // → /api/carts
    public class CartsController : ControllerBase
    {
        private readonly ICartRepository _repo;

        public CartsController(ICartRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Create a new cart.
        /// </summary>
        /// <remarks>
        /// Example request:
        /// {
        ///   "userId":  7,
        ///   "isCheckedOut": false
        /// }
        /// </remarks>
        /// <returns>201 Created with the new cart in the body.</returns>
        [HttpPost]
        public async Task<ActionResult<Cart>> CreateAsync([FromBody] Cart cart)
        {
            if (cart is null) return BadRequest();
            if (cart.UserId <= 0) return BadRequest("UserId must be positive.");

            int newId = await _repo.CreateAsync(cart);
            cart.Id = newId;                  // reflect DB-generated Id

            // 201 Created + Location header (no GET route yet ⇒ routeName = null)
            return CreatedAtRoute(
                routeName: null,
                routeValues: new { id = newId },
                value: cart);
        }


    }
}
