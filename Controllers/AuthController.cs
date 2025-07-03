using Microsoft.AspNetCore.Mvc;
using UserApi.Models;
using UserApi.Data;
using ProductApi.Services;
using UserApi.Services;
using ProductApi.Configuration;
namespace UserApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;
        private readonly AppSettings _appSettings;


        public AuthController(IUserRepository userRepository, ITokenService tokenService, IEmailService emailService, ILogger<AuthController> logger,
    AppSettings appSettings)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _emailService = emailService;
            _logger = logger;
            _appSettings = appSettings;
        }



        /// <summary>
        /// Login dengan email dan password
        /// POST /api/auth/login
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Login attempt for email: {request.Email}");

                // Validasi input
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Data tidak valid"
                    });
                }

                // Cari user berdasarkan email
                var user = await _userRepository.GetUserByEmailAsync(request.Email);

                if (user == null)
                {
                    _logger.LogWarning($"Login failed: User not found for email {request.Email}");
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Email atau password salah"
                    });
                }
                _logger.LogInformation($"user.Password: {user.Password}");
                _logger.LogInformation($"request.Password: {request.Password}");

                // Verifikasi password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    _logger.LogWarning($"Login failed: Invalid password for email {request.Email}");
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Email atau password salah"
                    });
                }

                // Update last login date
                await _userRepository.UpdateLastLoginAsync(user.Id);

                // Generate JWT
                string token = _tokenService.GenerateToken(user);

                _logger.LogInformation($"Login successful for email: {request.Email}");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login berhasil",
                    User = new UserInfo
                    {
                        UserID = user.Id,
                        Email = user.Email,
                        Username = user.Username,
                        LastLoginDate = DateTime.Now
                    },
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for email: {request.Email}");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Terjadi kesalahan server"
                });
            }
        }

        /// <summary>
        /// Register user baru
        /// POST /api/auth/register
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation($"Register attempt for email: {request.Email}");

                // Validasi input
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Data tidak valid"
                    });
                }

                // Cek apakah email sudah ada
                bool emailExists = await _userRepository.EmailExistsAsync(request.Email);
                if (emailExists)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Email sudah terdaftar"
                    });
                }

                // Buat user baru
                bool created = await _userRepository.CreateUserAsync(request);

                if (!created)
                {
                    return StatusCode(500, new LoginResponse
                    {
                        Success = false,
                        Message = "Gagal membuat akun"
                    });
                }

                // Get the newly created user to generate token
                var newUser = await _userRepository.GetUserByEmailAsync(request.Email);
                if (newUser == null)
                {
                    return StatusCode(500, new LoginResponse
                    {
                        Success = false,
                        Message = "Gagal mengambil data user yang baru dibuat"
                    });
                }

                // // Generate email verification token
                // string verificationToken = await _userRepository.UpdateEmailVerificationTokenAsync(newUser.Id);

                // // buat link verifikasi (contoh)
                // string verificationLink = $"https://yourfrontend.com/verify?token={verificationToken}";

                // // kirim email
                // await _emailService.SendEmailAsync(newUser.Email, "Verifikasi Akun Anda",
                //     $"Klik link berikut untuk verifikasi akun Anda: <a href='{verificationLink}'>Verifikasi</a>");

                // Generate JWT token (optional, kalau mau auto login)
                // string token = _tokenService.GenerateToken(newUser);

                // _logger.LogInformation($"Register successful for email: {request.Email}, verification email sent.");


                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Akun berhasil dibuat dan login otomatis",
                    User = new UserInfo
                    {
                        UserID = newUser.Id,
                        Email = newUser.Email,
                        Username = newUser.Username,
                        LastLoginDate = DateTime.Now
                    },
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during register for email: {request.Email}");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Terjadi kesalahan server"
                });
            }
        }

        /// <summary>
        /// Logout (placeholder untuk future implementation)
        /// POST /api/auth/logout
        /// </summary>
        [HttpPost("logout")]
        public ActionResult<LoginResponse> Logout()
        {
            // Untuk simple token, logout hanya mengembalikan response success
            // Untuk JWT, token bisa ditambahkan ke blacklist

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Logout berhasil"
            });
        }

        [HttpGet("verify-email")]
        public async Task<ActionResult<LoginResponse>> VerifyEmail([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Token verifikasi tidak valid"
                    });
                }

                // Verify email with token
                bool verified = await _userRepository.VerifyEmailAsync(token);

                if (!verified)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Token verifikasi tidak valid atau sudah expired"
                    });
                }

                _logger.LogInformation($"Email verification successful for token: {token.Substring(0, 8)}...");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Email berhasil diverifikasi. Silakan login dengan akun Anda."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during email verification for token: {token?.Substring(0, 8)}...");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Terjadi kesalahan server"
                });
            }
        }

        /// <summary>
        /// Meminta reset password
        /// POST /api/auth/forgot-password
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<ActionResult<LoginResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Email tidak valid"
                    });
                }

                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return Ok(new LoginResponse
                    {
                        Success = true,
                        Message = "Jika email terdaftar, link reset password telah dikirim."
                    });
                }

                if (!user.IsActive)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Akun Anda belum aktif."
                    });
                }

                // generate token
                string resetToken = await _userRepository.UpdatePasswordResetTokenAsync(user.Id);

                // buat link reset
                string resetLink = $"{_appSettings.FrontendBaseUrl}/create-new-password?token={resetToken}";


                // kirim email
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.Username, resetToken);


                _logger.LogInformation($"Password reset email sent to {user.Email}");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Jika email terdaftar, link reset password telah dikirim."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during forgot password for email: {request.Email}");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Terjadi kesalahan server"
                });
            }
        }

        /// <summary>
        /// Reset password menggunakan token
        /// POST /api/auth/reset-password
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<ActionResult<LoginResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Data tidak valid"
                    });
                }

                var user = await _userRepository.GetUserByPasswordResetTokenAsync(request.Token);
                if (user == null)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Token reset password tidak valid atau sudah expired"
                    });
                }

                // hash password baru
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                bool updated = await _userRepository.UpdatePasswordAndClearResetTokenAsync(user.Id, hashedPassword);
                if (!updated)
                {
                    return StatusCode(500, new LoginResponse
                    {
                        Success = false,
                        Message = "Gagal memperbarui password"
                    });
                }

                _logger.LogInformation($"Password reset successful for user {user.Email}");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Password berhasil direset. Silakan login dengan password baru."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during reset password with token: {request.Token}");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Terjadi kesalahan server"
                });
            }
        }

    }
}
