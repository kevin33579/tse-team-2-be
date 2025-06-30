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
    }
}
