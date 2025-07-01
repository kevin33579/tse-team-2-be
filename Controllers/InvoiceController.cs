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
    }
}
