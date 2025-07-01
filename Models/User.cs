using System;
using System.ComponentModel.DataAnnotations;

namespace UserApi.Models
{
        public class User
        {
                public int Id { get; set; }
                [Required]
                [StringLength(100)]
                [DataType(DataType.Password)]
                [Display(Name = "")]
                public required string Username { get; set; }
                public required string Password { get; set; }
                public required string Email { get; set; }
                public int RoleID { get; set; }

                public DateTime CreatedDate { get; set; } = DateTime.Now;
                public DateTime? LastLoginDate { get; set; }
                public bool IsActive { get; set; } = true;

                public bool IsEmailVerified { get; set; } = false;
                public string? EmailVerificationToken { get; set; }
                public DateTime? EmailTokenCreatedAt { get; set; }
                public string? PasswordResetToken { get; set; }
                public DateTime? PasswordResetTokenCreatedAt { get; set; }
        }

        public class LoginRequest
        {
                [Required]
                [EmailAddress]
                public string Email { get; set; } = string.Empty;

                [Required]
                public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
                public bool Success { get; set; }
                public string Message { get; set; } = string.Empty;
                public UserInfo? User { get; set; }
                public string? Token { get; set; }
        }
        public class UserInfo
        {
                public int UserID { get; set; }
                public string Email { get; set; } = string.Empty;
                public string? Username { get; set; }
                public DateTime? LastLoginDate { get; set; }
        }

        public class RegisterRequest
        {
                [Required]
                [EmailAddress]
                public string Email { get; set; } = string.Empty;

                [Required]
                [MinLength(6)]
                public string Password { get; set; } = string.Empty;

                public string? Username { get; set; }

        }
        public class ForgotPasswordRequest
        {
                [Required]
                [EmailAddress]
                public string Email { get; set; } = string.Empty;
        }

        public class ResetPasswordRequest
        {
                [Required]
                public string Token { get; set; } = string.Empty;

                [Required]
                [MinLength(6)]
                public string NewPassword { get; set; } = string.Empty;

                [Required]
                [Compare("NewPassword", ErrorMessage = "Password dan konfirmasi password tidak sama")]
                public string ConfirmPassword { get; set; } = string.Empty;
        }
}