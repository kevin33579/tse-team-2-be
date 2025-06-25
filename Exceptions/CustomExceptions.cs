namespace ProductApi.Exceptions
{
    /// <summary>
    /// Base class untuk semua custom exception di aplikasi
    /// Menyediakan struktur dasar untuk exception handling yang konsisten
    /// Inherit dari System.Exception untuk kompatibilitas dengan .NET exception handling
    /// </summary>
    public abstract class CustomException : Exception
    {
        /// <summary>
        /// Constructor dengan pesan error
        /// </summary>
        /// <param name="message">Pesan error yang menjelaskan masalah yang terjadi</param>
        protected CustomException(string message) : base(message) { }
        
        /// <summary>
        /// Constructor dengan pesan error dan inner exception
        /// </summary>
        /// <param name="message">Pesan error yang menjelaskan masalah yang terjadi</param>
        /// <param name="innerException">Exception asli yang menyebabkan error ini (untuk debugging)</param>
        protected CustomException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception untuk kesalahan validasi input dari user
    /// Digunakan ketika data yang dikirim user tidak sesuai dengan aturan bisnis
    /// Contoh: field required kosong, format email salah, nilai di luar range, dll
    /// </summary>
    public class ValidationException : CustomException
    {
        /// <summary>
        /// Daftar error validasi yang terjadi
        /// Bisa berisi multiple error sekaligus (contoh: nama kosong DAN email invalid)
        /// </summary>
        public List<string> Errors { get; }

        /// <summary>
        /// Constructor untuk single validation error
        /// </summary>
        /// <param name="message">Pesan error validasi</param>
        public ValidationException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        /// <summary>
        /// Constructor untuk multiple validation errors
        /// </summary>
        /// <param name="errors">List semua error validasi yang ditemukan</param>
        public ValidationException(List<string> errors) : base("Validation failed")
        {
            Errors = errors;
        }

        /// <summary>
        /// Constructor untuk validation error dengan field spesifik
        /// </summary>
        /// <param name="field">Nama field yang error (contoh: "email", "password")</param>
        /// <param name="message">Pesan error untuk field tersebut</param>
        public ValidationException(string field, string message) : base($"Validation failed for {field}")
        {
            Errors = new List<string> { $"{field}: {message}" };
        }
    }

    /// <summary>
    /// Exception untuk resource yang tidak ditemukan (HTTP 404)
    /// Digunakan ketika user meminta data yang tidak ada di database
    /// Contoh: Product ID 999 tidak ditemukan, User dengan email xxx tidak ada
    /// </summary>
    public class NotFoundException : CustomException
    {
        /// <summary>
        /// Constructor dengan nama resource dan key yang dicari
        /// </summary>
        /// <param name="resource">Nama resource (contoh: "Product", "User")</param>
        /// <param name="key">Key/ID yang dicari (contoh: 123, "admin@email.com")</param>
        public NotFoundException(string resource, object key) 
            : base($"{resource} with key '{key}' was not found") { }

        /// <summary>
        /// Constructor dengan custom message
        /// </summary>
        /// <param name="message">Custom pesan not found</param>
        public NotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception untuk masalah authentication (HTTP 401)
    /// Digunakan ketika user belum login atau token tidak valid
    /// Contoh: JWT token expired, login credential salah, token tidak ada
    /// </summary>
    public class UnauthorizedException : CustomException
    {
        /// <summary>
        /// Constructor dengan pesan default atau custom
        /// </summary>
        /// <param name="message">Pesan unauthorized (default: "Unauthorized access")</param>
        public UnauthorizedException(string message = "Unauthorized access") : base(message) { }
    }

    /// <summary>
    /// Exception untuk masalah authorization (HTTP 403)
    /// Digunakan ketika user sudah login tapi tidak punya hak akses
    /// Contoh: user biasa coba akses admin panel, role tidak sesuai
    /// </summary>
    public class ForbiddenException : CustomException
    {
        /// <summary>
        /// Constructor dengan pesan default atau custom
        /// </summary>
        /// <param name="message">Pesan forbidden (default: "Access forbidden")</param>
        public ForbiddenException(string message = "Access forbidden") : base(message) { }
    }

    /// <summary>
    /// Exception untuk error business logic/domain rules
    /// Digunakan ketika operasi melanggar aturan bisnis yang kompleks
    /// Contoh: stok habis, tanggal tidak valid, operasi tidak diizinkan dalam status tertentu
    /// </summary>
    public class BusinessLogicException : CustomException
    {
        /// <summary>
        /// Kode error spesifik untuk business logic (untuk debugging dan kategorisasi)
        /// Contoh: "INSUFFICIENT_STOCK", "INVALID_DATE_RANGE", "ORDER_ALREADY_COMPLETED"
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Constructor dengan pesan error (error code default)
        /// </summary>
        /// <param name="message">Pesan business logic error</param>
        public BusinessLogicException(string message) : base(message)
        {
            ErrorCode = "BUSINESS_LOGIC_ERROR";
        }

        /// <summary>
        /// Constructor dengan custom error code dan pesan
        /// </summary>
        /// <param name="errorCode">Kode error spesifik (contoh: "INSUFFICIENT_STOCK")</param>
        /// <param name="message">Pesan business logic error</param>
        public BusinessLogicException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// Exception untuk error database operations
    /// Digunakan ketika terjadi masalah dengan operasi database
    /// Contoh: connection timeout, constraint violation, deadlock, SQL syntax error
    /// </summary>
    public class DatabaseException : CustomException
    {
        /// <summary>
        /// Operasi database yang sedang dijalankan saat error
        /// Contoh: "INSERT", "UPDATE", "DELETE", "SELECT"
        /// </summary>
        public string? Operation { get; }

        /// <summary>
        /// Constructor dengan pesan error saja
        /// </summary>
        /// <param name="message">Pesan database error</param>
        public DatabaseException(string message) : base(message) { }

        /// <summary>
        /// Constructor dengan operasi dan pesan error
        /// </summary>
        /// <param name="operation">Operasi yang sedang dijalankan (INSERT/UPDATE/DELETE/SELECT)</param>
        /// <param name="message">Pesan database error</param>
        public DatabaseException(string operation, string message) : base(message)
        {
            Operation = operation;
        }

        /// <summary>
        /// Constructor dengan operasi, pesan, dan inner exception
        /// </summary>
        /// <param name="operation">Operasi yang sedang dijalankan</param>
        /// <param name="message">Pesan database error</param>
        /// <param name="innerException">Exception asli dari database driver</param>
        public DatabaseException(string operation, string message, Exception innerException) 
            : base(message, innerException)
        {
            Operation = operation;
        }
    }

    /// <summary>
    /// Exception untuk konflik resource (HTTP 409)
    /// Digunakan ketika operasi bertentangan dengan data yang sudah ada
    /// Contoh: email sudah terdaftar, nama produk sudah ada, duplicate key
    /// </summary>
    public class ConflictException : CustomException
    {
        /// <summary>
        /// Constructor dengan pesan conflict
        /// </summary>
        /// <param name="message">Pesan yang menjelaskan konflik yang terjadi</param>
        public ConflictException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception untuk error pada external service/API calls
    /// Digunakan ketika terjadi masalah dengan service eksternal
    /// Contoh: payment gateway down, email service error, third-party API timeout
    /// </summary>
    public class ExternalServiceException : CustomException
    {
        /// <summary>
        /// Nama service eksternal yang bermasalah
        /// Contoh: "PaymentGateway", "EmailService", "GoogleMapsAPI"
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Constructor dengan nama service dan pesan error
        /// </summary>
        /// <param name="serviceName">Nama service eksternal yang error</param>
        /// <param name="message">Pesan error dari service tersebut</param>
        public ExternalServiceException(string serviceName, string message) : base(message)
        {
            ServiceName = serviceName;
        }

        /// <summary>
        /// Constructor dengan nama service, pesan, dan inner exception
        /// </summary>
        /// <param name="serviceName">Nama service eksternal yang error</param>
        /// <param name="message">Pesan error dari service tersebut</param>
        /// <param name="innerException">Exception asli dari service call</param>
        public ExternalServiceException(string serviceName, string message, Exception innerException) 
            : base(message, innerException)
        {
            ServiceName = serviceName;
        }
    }

    /// <summary>
    /// Exception untuk error file operations
    /// Digunakan ketika terjadi masalah dengan operasi file
    /// Contoh: file not found, access denied, disk full, invalid file format
    /// </summary>
    public class FileOperationException : CustomException
    {
        /// <summary>
        /// Path file yang sedang dioperasikan
        /// </summary>
        public string FilePath { get; }
        
        /// <summary>
        /// Operasi file yang sedang dijalankan
        /// Contoh: "READ", "WRITE", "DELETE", "UPLOAD", "DOWNLOAD"
        /// </summary>
        public string Operation { get; }

        /// <summary>
        /// Constructor dengan operasi, path file, dan pesan error
        /// </summary>
        /// <param name="operation">Operasi yang sedang dijalankan (READ/WRITE/DELETE/dll)</param>
        /// <param name="filePath">Path file yang bermasalah</param>
        /// <param name="message">Pesan error file operation</param>
        public FileOperationException(string operation, string filePath, string message) : base(message)
        {
            Operation = operation;
            FilePath = filePath;
        }
    }
}
