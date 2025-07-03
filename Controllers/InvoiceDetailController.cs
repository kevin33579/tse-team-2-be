using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InvoiceDetailApi.Data;
using InvoiceDetailApi.Models;
using ProductApi.Exceptions;

namespace YourApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceDetailController : ControllerBase
    {
        private readonly IInvoiceDetailRepository _repo;

        public InvoiceDetailController(IInvoiceDetailRepository repo)
        {
            _repo = repo;
        }

        // POST: api/InvoiceDetail
        [HttpPost]
        public async Task<ActionResult> CreateMany([FromBody] List<DetailInvoice> details, CancellationToken ct)
        {
            if (details == null || details.Count == 0)
                return BadRequest(new { message = "Detail list cannot be empty." });

            try
            {
                var count = await _repo.CreateManyAsync(details, ct);
                return Ok(new { message = $"{count} detail(s) created." });
            }
            catch (DatabaseException ex)
            {
                return StatusCode(500, new { message = "Failed to insert invoice details.", ex.Message });
            }
        }

        // GET: api/InvoiceDetail/invoice/5
        [HttpGet("invoice/{invoiceId}")]
        public async Task<ActionResult<IEnumerable<DetailInvoice>>> GetByInvoiceId(uint invoiceId, CancellationToken ct)
        {
            try
            {
                var details = await _repo.GetByInvoiceIdAsync(invoiceId, ct);
                return Ok(details);
            }
            catch (DatabaseException ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to insert invoice details.",
                    error = ex.Message
                });
            }
        }

        // GET: api/InvoiceDetail/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<InvoiceDetailSummaryDto>>> GetByUser(uint userId, CancellationToken ct)
        {
            try
            {
                var rows = await _repo.GetByUserIdAsync(userId, ct);
                return Ok(rows);
            }
            catch (DatabaseException ex)
            {
                return StatusCode(500, new { message = "Failed to fetch user invoice details.", error = ex.Message });
            }
        }

    }
}
