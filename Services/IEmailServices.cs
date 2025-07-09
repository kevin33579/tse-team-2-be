using UserApi.Models;

namespace UserApi.Services
{
    /// <summary>
    /// Interface untuk email service yang menghandle pengiriman email
    /// Digunakan untuk verification, reset password, notifications, dll
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Mengirim email verification ke user yang baru register
        /// </summary>
        /// <param name="to">Email tujuan</param>
        /// <param name="userName">Nama user untuk personalisasi</param>
        /// <param name="verificationToken">Token verification yang akan di-embed di link</param>
        /// <returns>True jika berhasil kirim, false jika gagal</returns>
        Task<bool> SendVerificationEmailAsync(string to, string userName, string verificationToken);
        
        /// <summary>
        /// Mengirim email reset password dengan link reset
        /// </summary>
        /// <param name="to">Email tujuan</param>
        /// <param name="userName">Nama user</param>
        /// <param name="resetToken">Token reset password</param>
        /// <returns>True jika berhasil</returns>
        Task<bool> SendPasswordResetEmailAsync(string to, string userName, string resetToken);
        
        /// <summary>
        /// Mengirim email konfirmasi order kepada customer
        /// </summary>
        /// <param name="to">Email customer</param>
        /// <param name="userName">Nama customer</param>
        /// <param name="orderNumber">Nomor order</param>
        /// <param name="totalAmount">Total pembayaran</param>
        /// <returns>True jika berhasil</returns>
        Task<bool> SendOrderConfirmationEmailAsync(string to, string userName, string orderNumber, decimal totalAmount);
        
        /// <summary>
        /// Generic method untuk mengirim email dengan template custom
        /// </summary>
        /// <param name="to">Email tujuan</param>
        /// <param name="subject">Subject email</param>
        /// <param name="htmlBody">HTML body content</param>
        /// <param name="textBody">Plain text fallback (optional)</param>
        /// <returns>True jika berhasil</returns>
        Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null);
    }
}
