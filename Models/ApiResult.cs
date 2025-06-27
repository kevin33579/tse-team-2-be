namespace ProductApi.Models
{
    // Generic result wrapper for API responses
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Success response with data
        public static ApiResult<T> SuccessResult(T data, string? message = null, int statusCode = 200)
        {
            return new ApiResult<T>
            {
                Success = true,
                Data = data,
                Message = message ?? "Operation completed successfully",
                StatusCode = statusCode
            };
        }

        // Success response without data
        public static ApiResult<T> SuccessResult(string message = "Operation completed successfully", int statusCode = 200)
        {
            return new ApiResult<T>
            {
                Success = true,
                Message = message,
                StatusCode = statusCode
            };
        }

        // Error response with single error
        public static ApiResult<T> ErrorResult(string error, int statusCode = 400)
        {
            return new ApiResult<T>
            {
                Success = false,
                Errors = new List<string> { error },
                Message = "Operation failed",
                StatusCode = statusCode
            };
        }

        // Error response with multiple errors
        public static ApiResult<T> ErrorResult(List<string> errors, int statusCode = 400)
        {
            return new ApiResult<T>
            {
                Success = false,
                Errors = errors,
                Message = "Operation failed",
                StatusCode = statusCode
            };
        }

        // Error response with custom message
        public static ApiResult<T> ErrorResult(string message, List<string> errors, int statusCode = 400)
        {
            return new ApiResult<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }
    }

    // Non-generic result wrapper
    public class ApiResult : ApiResult<object>
    {
        // Success response without data
        public static new ApiResult SuccessResult(string message = "Operation completed successfully", int statusCode = 200)
        {
            return new ApiResult
            {
                Success = true,
                Message = message,
                StatusCode = statusCode
            };
        }

        // Error response with single error
        public static new ApiResult ErrorResult(string error, int statusCode = 400)
        {
            return new ApiResult
            {
                Success = false,
                Errors = new List<string> { error },
                Message = "Operation failed",
                StatusCode = statusCode
            };
        }

        public static ApiResult<T> ErrorResult<T>(IEnumerable<string> errors, int statusCode)
    => new ApiResult<T>
    {
        Success = false,
        StatusCode = statusCode,
        Message = "Validation failed",
        Errors = errors.ToList()
    };

    }

    // Paginated result wrapper
    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }

        public static PaginatedResult<T> Create(List<T> data, int pageNumber, int pageSize, int totalRecords)
        {
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return new PaginatedResult<T>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };
        }
    }
}
