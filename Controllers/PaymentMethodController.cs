using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentApi.Models;
using PaymentApi.Data;      // IPaymentMethodRepository
using ProductApi.Exceptions;        // ApiResult (same namespace as your other controllers)
using ProductApi.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace PaymentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentMethodController : ControllerBase
    {
        private readonly IPaymentMethodRepository _repository;
        private readonly ILogger<PaymentMethodController> _logger;

        public PaymentMethodController(
            IPaymentMethodRepository repository,
            ILogger<PaymentMethodController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET: api/paymentmethod
        [HttpGet]
        public async Task<ActionResult<ApiResult<List<PaymentMethod>>>> GetAll(
            CancellationToken ct = default)
        {
            try
            {
                var methods = await _repository.GetAllAsync(ct);
                return Ok(ApiResult<List<PaymentMethod>>
                          .SuccessResult(methods, "Payment methods retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment methods");
                return StatusCode(500, ApiResult<List<PaymentMethod>>
                                   .ErrorResult("Server error", 500));
            }
        }
        // GET: api/paymentmethod/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<PaymentMethod>>> GetById(
            uint id,
            CancellationToken ct = default)
        {
            if (id == 0)
            {
                return BadRequest(ApiResult<PaymentMethod>
                                  .ErrorResult("Payment method ID cannot be zero", 400));  // Bad Request
            }

            try
            {
                var method = await _repository.GetPaymentMethodByIdAsync(id, ct);
                if (method == null)
                {
                    return NotFound(ApiResult<PaymentMethod>
                                    .ErrorResult("Payment method not found", 404));  // Not Found
                }
                return Ok(ApiResult<PaymentMethod>
                          .SuccessResult(method, "Payment method retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment method by ID");
                return StatusCode(500, ApiResult<PaymentMethod>
                                   .ErrorResult("Server error", 500));
            }
        }

        // POST: api/paymentmethod
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<PaymentMethod>>> Create(
            [FromBody] PaymentMethod paymentMethod,
            CancellationToken ct = default)
        {
            if (paymentMethod == null)
            {
                return BadRequest(ApiResult<PaymentMethod>
                                  .ErrorResult("Payment method cannot be null", 400));  // Bad Request
            }

            try
            {
                var createdMethod = await _repository.CreatePaymentMethodAsync(paymentMethod, ct);
                return Ok(ApiResult<PaymentMethod>
                          .SuccessResult(createdMethod, "Payment method created"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment method");
                return StatusCode(500, ApiResult<PaymentMethod>
                                   .ErrorResult("Server error", 500));
            }
        }
        // PUT: api/paymentmethod
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<PaymentMethod>>> Update(
            [FromBody] PaymentMethod paymentMethod,
            CancellationToken ct = default)
        {
            if (paymentMethod == null || paymentMethod.Id == 0)
            {
                return BadRequest(ApiResult<PaymentMethod>
                                  .ErrorResult("Payment method cannot be null or have zero ID", 400));  // Bad Request
            }

            try
            {
                var updatedMethod = await _repository.UpdatePaymentMethodAsync(paymentMethod, ct);
                return Ok(ApiResult<PaymentMethod>
                          .SuccessResult(updatedMethod, "Payment method updated"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment method");
                return StatusCode(500, ApiResult<PaymentMethod>
                                   .ErrorResult("Server error", 500));
            }
        }
        // DELETE: api/paymentmethod/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<string>>> Delete(
            uint id,
            CancellationToken ct = default)
        {
            if (id == 0)
            {
                return BadRequest(ApiResult<string>
                                  .ErrorResult("Payment method ID cannot be zero", 400));  // Bad Request
            }

            try
            {
                await _repository.DeletePaymentMethodAsync(id, ct);
                return Ok(ApiResult<string>
                          .SuccessResult($"Payment method with ID {id} deleted", "Payment method deleted"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment method");
                return StatusCode(500, ApiResult<string>
                                   .ErrorResult("Server error", 500));
            }
        }
    }
}
