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
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                // 1. validate payload
                if (!ModelState.IsValid)
                    return BadRequest(new LoginResponse { Success = false, Message = "Data tidak valid" });

                // 2. fetch user + role
                var result = await _userRepository.GetUserWithRoleByEmailAsync(request.Email);
                if (result == null)
                {
                    _logger.LogWarning("Login failed: user not found for {Email}", request.Email);
                    return Unauthorized(new LoginResponse { Success = false, Message = "Email atau password salah" });
                }

                var (user, roleName) = result.Value;

                // 3. verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    _logger.LogWarning("Login failed: wrong password for {Email}", request.Email);
                    return Unauthorized(new LoginResponse { Success = false, Message = "Email atau password salah" });
                }

                // 4. cek apakah email sudah diverifikasi
                if (!user.IsEmailVerified)
                {
                    _logger.LogWarning("Login blocked: email not verified for {Email}", request.Email);
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Akun Anda belum diverifikasi. Silakan cek email Anda untuk verifikasi."
                    });
                }

                // 5. update last login
                await _userRepository.UpdateLastLoginAsync(user.Id);

                // 6. generate JWT
                string token = _tokenService.GenerateToken(user, roleName);

                _logger.LogInformation("Login successful for {Email}", request.Email);

                // 7. success response
                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login berhasil",
                    User = new UserInfo
                    {
                        UserID = user.Id,
                        Email = user.Email,
                        Username = user.Username,
                        LastLoginDate = DateTime.Now,
                        RoleName = roleName
                    },
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", request.Email);
                return StatusCode(500, new LoginResponse { Success = false, Message = "Terjadi kesalahan server" });
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

                // 1. Validasi input
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Data tidak valid"
                    });
                }

                // 2. Cek apakah email sudah terdaftar
                bool emailExists = await _userRepository.EmailExistsAsync(request.Email);
                if (emailExists)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Email sudah terdaftar"
                    });
                }

                // 3. Buat user baru
                int? userId = await _userRepository.CreateUserAsync(request);
                if (userId == null)
                {
                    return StatusCode(500, new LoginResponse
                    {
                        Success = false,
                        Message = "Gagal membuat akun"
                    });
                }


                // 4. Ambil data user yang baru dibuat
                var newUser = await _userRepository.GetUserByIdAsync(userId.Value);
                if (newUser == null)
                {
                    return StatusCode(500, new LoginResponse
                    {
                        Success = false,
                        Message = "Gagal mengambil data user yang baru dibuat"
                    });
                }

                // 5. Generate token verifikasi dan simpan di DB
                string verificationToken = await _userRepository.UpdateEmailVerificationTokenAsync(newUser.Id);

                // 6. Kirim email verifikasi
                bool emailSent = await _emailService.SendVerificationEmailAsync(newUser.Email, newUser.Username, verificationToken);
                if (!emailSent)
                {
                    _logger.LogWarning("Gagal mengirim email verifikasi ke {Email}", newUser.Email);
                    return StatusCode(500, new LoginResponse
                    {
                        Success = false,
                        Message = "Akun berhasil dibuat, tapi gagal mengirim email verifikasi. Coba lagi nanti."
                    });
                }

                _logger.LogInformation($"Register successful for email: {request.Email}, verification email sent.");

                // 7. Return response sukses
                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Akun berhasil dibuat. Silakan verifikasi email Anda melalui link yang dikirim.",
                    User = new UserInfo
                    {
                        UserID = newUser.Id,
                        Email = newUser.Email,
                        Username = newUser.Username,
                        LastLoginDate = DateTime.Now
                    }
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
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token verifikasi tidak valid");
                }

                // Verifikasi email
                bool verified = await _userRepository.VerifyEmailAsync(token);

                if (!verified)
                {
                    return BadRequest("Token verifikasi tidak valid atau sudah expired");
                }

                _logger.LogInformation($"Email verification successful for token: {token.Substring(0, 8)}...");

                // ✅ Redirect ke login page frontend
                return Redirect("http://localhost:5173/login?verified=true");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during email verification for token: {token?.Substring(0, 8)}...");
                return StatusCode(500, "Terjadi kesalahan server");
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
