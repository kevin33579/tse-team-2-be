using System.Net;
using System.Text.Json;
using ProductApi.Exceptions;

namespace ProductApi.Middleware
{
    /// <summary>
    /// Global Exception Handling Middleware untuk menangkap dan memproses semua exception
    /// yang tidak ditangani di level controller atau service
    /// 
    /// Middleware ini berfungsi sebagai "safety net" untuk:
    /// - Mencegah aplikasi crash karena unhandled exception
    /// - Memberikan response yang konsisten untuk semua jenis error
    /// - Logging semua error untuk debugging dan monitoring
    /// - Menyembunyikan detail teknis error dari user (security)
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        /// <summary>
        /// Delegate untuk memanggil middleware berikutnya dalam pipeline
        /// ASP.NET Core menggunakan pattern middleware chain
        /// </summary>
        private readonly RequestDelegate _next;
        
        /// <summary>
        /// Logger untuk mencatat semua exception yang terjadi
        /// Penting untuk debugging, monitoring, dan audit
        /// </summary>
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Constructor untuk dependency injection
        /// </summary>
        /// <param name="next">Middleware selanjutnya dalam pipeline</param>
        /// <param name="logger">Logger service untuk pencatatan error</param>
        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Method utama yang dipanggil untuk setiap HTTP request
        /// Membungkus seluruh request processing dengan try-catch
        /// </summary>
        /// <param name="context">HTTP context yang berisi informasi request dan response</param>
        /// <returns>Task untuk async operation</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Lanjutkan ke middleware berikutnya dalam pipeline
                // Jika tidak ada exception, request akan diproses normal
                await _next(context);
            }
            catch (Exception ex)
            {
                // Tangkap semua exception yang tidak ditangani di level lain
                // Log error dengan detail request untuk debugging
                _logger.LogError(ex, "An unhandled exception occurred. Request: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                
                // Proses exception dan kirim response yang sesuai ke client
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Method untuk memproses exception dan mengkonversinya menjadi HTTP response
        /// Mapping setiap jenis exception ke HTTP status code dan message yang sesuai
        /// </summary>
        /// <param name="context">HTTP context untuk menulis response</param>
        /// <param name="exception">Exception yang akan diproses</param>
        /// <returns>Task untuk async operation</returns>
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Set content type sebagai JSON untuk response yang konsisten
            context.Response.ContentType = "application/json";

            // Buat object response yang akan dikirim ke client
            var response = new ErrorResponse();

            // Mapping exception ke HTTP status code dan message
            // Menggunakan pattern matching untuk handle berbagai jenis exception
            switch (exception)
            {
                case ValidationException validationEx:
                    // HTTP 400 Bad Request - untuk input validation errors
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Validation failed";
                    response.Details = validationEx.Errors; // Detail validation errors
                    break;

                case NotFoundException notFoundEx:
                    // HTTP 404 Not Found - untuk resource yang tidak ditemukan
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = notFoundEx.Message;
                    break;

                case UnauthorizedException unauthorizedEx:
                    // HTTP 401 Unauthorized - untuk authentication failures
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Unauthorized access";
                    response.Details = unauthorizedEx.Message;
                    break;

                case ForbiddenException forbiddenEx:
                    // HTTP 403 Forbidden - untuk authorization failures
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response.Message = "Access forbidden";
                    response.Details = forbiddenEx.Message;
                    break;

                case BusinessLogicException businessEx:
                    // HTTP 400 Bad Request - untuk business logic violations
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Business logic error";
                    response.Details = businessEx.Message;
                    break;

                case DatabaseException dbEx:
                    // HTTP 500 Internal Server Error - untuk database errors
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "Database operation failed";
                    // Jangan expose detail database error ke user (security)
                    response.Details = "A database error occurred. Please try again later.";
                    break;

                case TimeoutException timeoutEx:
                    // HTTP 408 Request Timeout - untuk operation timeout
                    response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response.Message = "Request timeout";
                    response.Details = "The request took too long to process.";
                    break;

                default:
                    // HTTP 500 Internal Server Error - untuk semua exception lainnya
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "An internal server error occurred";
                    // Generic message untuk security (jangan expose technical details)
                    response.Details = "Something went wrong. Please contact support if the problem persists.";
                    break;
            }

            // Set HTTP status code pada response
            context.Response.StatusCode = response.StatusCode;

            // Serialize response object menjadi JSON dengan formatting yang konsisten
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // camelCase untuk frontend
                WriteIndented = true // Format JSON yang readable untuk debugging
            });

            // Kirim JSON response ke client
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Model untuk error response yang konsisten
    /// Semua error akan menggunakan format response yang sama
    /// 
    /// Contoh response JSON:
    /// {
    ///   "statusCode": 400,
    ///   "message": "Validation failed",
    ///   "details": ["Name is required", "Email format invalid"],
    ///   "timestamp": "2025-06-25T10:30:00Z",
    ///   "traceId": "12345-67890-abcdef"
    /// }
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// HTTP status code (400, 404, 500, dll)
        /// Sama dengan HTTP response status code
        /// </summary>
        public int StatusCode { get; set; }
        
        /// <summary>
        /// Pesan error utama yang user-friendly
        /// Contoh: "Validation failed", "Resource not found"
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Detail error atau additional information
        /// Bisa berupa string, array, atau object tergantung jenis error
        /// Untuk ValidationException: array of validation errors
        /// Untuk exception lain: string message
        /// </summary>
        public object? Details { get; set; }
        
        /// <summary>
        /// Timestamp kapan error terjadi dalam format ISO 8601 UTC
        /// Berguna untuk debugging dan correlation dengan log
        /// </summary>
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        
        /// <summary>
        /// Trace ID untuk correlation dengan distributed tracing
        /// Akan diset otomatis oleh ASP.NET Core jika tracing diaktifkan
        /// Berguna untuk debugging di microservices environment
        /// </summary>
        public string? TraceId { get; set; }
    }
}
