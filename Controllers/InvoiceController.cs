using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InvoiceApi.Data;       // IInvoiceRepository
using InvoiceApi.Models;     // Invoice
using ProductApi.Exceptions;
using ProductApi.Models; // ApiResult (adjust namespace if needed)
using Microsoft.AspNetCore.Authorization; // For authorization attributes

namespace InvoiceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]               // →  api/invoice
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceRepository _repo;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            IInvoiceRepository repo,
            ILogger<InvoiceController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────
        // POST api/invoice
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<ActionResult<ApiResult<uint>>> CreateAsync(
            [FromBody] Invoice dto,
            CancellationToken ct = default)
        {
            try
            {
                var newId = await _repo.CreateAsync(dto, ct);
                return Ok(ApiResult<uint>.SuccessResult(
                          newId, "Invoice created"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return StatusCode(500, ApiResult<uint>.ErrorResult(
                                   "Server error", 500));
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET api/invoice/user/{userId:int}
        // ─────────────────────────────────────────────────────────────
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<ApiResult<List<Invoice>>>> GetByUserAsync(
            int userId,                           // note: int here
            CancellationToken ct = default)
        {
            try
            {
                // Cast to uint if your repo expects it
                var list = await _repo.GetByUserIdAsync((uint)userId, ct);

                return Ok(ApiResult<List<Invoice>>.SuccessResult(
                          list, "Invoices retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoices for user {UserId}", userId);
                return StatusCode(500, ApiResult<List<Invoice>>.ErrorResult(
                                   "Server error", 500));
            }
        }
        // ─────────────────────────────────────────────────────────────
        // GET api/invoice/{id:int}
        // ─────────────────────────────────────────────────────────────
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<Invoice>>> GetByIdAsync(
            int id,                                // note: int here
            CancellationToken ct = default)
        {
            try
            {
                // Cast to uint if your repo expects it
                var invoice = await _repo.GetByIdAsync((uint)id, ct);

                return Ok(ApiResult<Invoice>.SuccessResult(
                          invoice, "Invoice retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoice {InvoiceId}", id);
                return StatusCode(500, ApiResult<Invoice>.ErrorResult(
                                   "Server error", 500));
            }
        }
        // ─────────────────────────────────────────────────────────────
        // DELETE api/invoice/{id:int}
        // ─────────────────────────────────────────────────────────────
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<bool>>> DeleteAsync(
            int id,                                // note: int here
            CancellationToken ct = default)
        {
            try
            {
                // Cast to uint if your repo expects it
                var success = await _repo.DeleteAsync((uint)id, ct);

                return Ok(ApiResult<bool>.SuccessResult(
                          success, "Invoice deleted"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice {InvoiceId}", id);
                return StatusCode(500, ApiResult<bool>.ErrorResult(
                                   "Server error", 500));
            }
        }
        // ─────────────────────────────────────────────────────────────
        // PUT api/invoice
        // ─────────────────────────────────────────────────────────────
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<Invoice>>> UpdateAsync(
            [FromBody] Invoice dto,
            CancellationToken ct = default)
        {
            if (dto == null || dto.Id == 0)
            {
                return BadRequest(ApiResult<Invoice>
                                  .ErrorResult("Invoice cannot be null or have zero ID", 400));  // Bad Request
            }

            try
            {
                var updatedInvoice = await _repo.UpdateAsync(dto, ct);
                return Ok(ApiResult<Invoice>
                          .SuccessResult(updatedInvoice, "Invoice updated"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice");
                return StatusCode(500, ApiResult<Invoice>
                                   .ErrorResult("Server error", 500));
            }
        }
        // ─────────────────────────────────────────────────────────────
        // GET api/invoice/all
        // ─────────────────────────────────────────────────────────────
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<List<Invoice>>>> GetAllAsync(
            CancellationToken ct = default)
        {
            try
            {
                var invoices = await _repo.GetAllAsync(ct);
                return Ok(ApiResult<List<Invoice>>.SuccessResult(
                          invoices, "All invoices retrieved"));  // OK
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all invoices");
                return StatusCode(500, ApiResult<List<Invoice>>.ErrorResult(
                                   "Server error", 500));  // Internal Server Error
            }
        }
    }
}
