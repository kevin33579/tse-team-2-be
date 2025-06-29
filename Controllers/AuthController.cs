using Microsoft.AspNetCore.Mvc;
using UserApi.Models;
using UserApi.Data;
using ProductApi.Services;

namespace UserApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserRepository userRepository, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _logger = logger;
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

                // Generate JWT token for the new user
                string token = _tokenService.GenerateToken(newUser);

                _logger.LogInformation($"Register successful for email: {request.Email}");

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
                    Token = token
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
    }
}
