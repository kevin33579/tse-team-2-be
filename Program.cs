// Import namespace untuk ProductRepository
using UserApi.Data;
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
=======
>>>>>>> 69e4304 (a)
<<<<<<< HEAD

=======
<<<<<<< HEAD
>>>>>>> 00b6af1 (User)
<<<<<<< HEAD
>>>>>>> 75190de (User)
=======
=======
>>>>>>> 0cb494c (a)
>>>>>>> 69e4304 (a)
=======
>>>>>>> e792068 (add to cart)
=======
>>>>>>> 9cbec9e (commit)
using ProductApi.Data;
using ProductTypeApi.Data;
using ProductApi.Configuration;
using ProductApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProductApi.Services;
// =====================================
// BUILDER PATTERN - Konfigurasi Services
// =====================================
// WebApplicationBuilder = builder pattern untuk konfigurasi aplikasi web
var builder = WebApplication.CreateBuilder(args);

// AddControllers() = mendaftarkan services untuk MVC Controllers
// Tanpa ini, controller tidak akan berfungsi
builder.Services.AddControllers();

// AddEndpointsApiExplorer() = untuk metadata endpoints API
// Diperlukan untuk Swagger documentation
builder.Services.AddEndpointsApiExplorer();

// AddSwaggerGen() = mendaftarkan Swagger generator
// Swagger = tools untuk generate dokumentasi API otomatis
builder.Services.AddSwaggerGen(options =>
{
    // Konfigurasi informasi API
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Product API",
        Version = "v1",
        Description = "API untuk manajemen produk dengan fitur checkout dan payment",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@yourcompany.com"
        }
    });

    // Konfigurasi JWT Bearer Authentication
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Masukkan JWT token dalam format: Bearer {your-token}"
    });

    // Require authentication untuk semua endpoints
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });

    // Include XML comments jika ada
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key not found in configuration")))
    };
});


// =====================================
// CONFIGURATION SETTINGS REGISTRATION
// =====================================
// Configure strongly typed settings objects
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<SecuritySettings>(
    builder.Configuration.GetSection("SecuritySettings"));
builder.Services.Configure<FileUploadSettings>(
    builder.Configuration.GetSection("FileUploadSettings"));

// =====================================
// DEPENDENCY INJECTION REGISTRATION
// =====================================
// AddScoped = register service dengan Scoped lifetime
// Scoped = 1 instance per HTTP request
// IProductRepository akan di-resolve ke ProductRepository
// Setiap kali controller butuh IProductRepository, DI container akan provide ProductRepository instance
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductTypeRepository, ProductTypeRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

// =====================================
// CORS CONFIGURATION
// =====================================
// CORS = Cross-Origin Resource Sharing
// Diperlukan jika frontend dan backend di domain/port yang berbeda
builder.Services.AddCors(options =>
{
    // AddDefaultPolicy = kebijakan CORS default
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()      // Boleh dari origin mana saja (untuk development)
              .AllowAnyMethod()      // Boleh HTTP method apa saja (GET, POST, PUT, DELETE)
              .AllowAnyHeader();     // Boleh header apa saja
    });
});

// =====================================
// BUILD APPLICATION
// =====================================
// Build() = membuat WebApplication instance dari konfigurasi yang sudah didefinisikan
var app = builder.Build();

// =====================================
// HTTP REQUEST PIPELINE CONFIGURATION
// =====================================
// Pipeline = urutan middleware yang akan memproses setiap HTTP request
// Urutan middleware PENTING! Request akan melewati middleware dari atas ke bawah

// Environment check - hanya aktif di Development environment
if (app.Environment.IsDevelopment())
{
    // UseSwagger() = middleware untuk expose Swagger JSON endpoint
    // Biasanya di: /swagger/v1/swagger.json
    app.UseSwagger();

    // UseSwaggerUI() = middleware untuk Swagger UI web interface
    // Biasanya di: /swagger/index.html
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
// UseHttpsRedirection() = middleware untuk redirect HTTP ke HTTPS
// Security best practice - semua request akan di-redirect ke HTTPS
app.UseHttpsRedirection();

// UseCors() = middleware untuk enable CORS policy yang sudah dikonfigurasi
// Harus dipanggil sebelum UseAuthorization dan MapControllers
app.UseCors();

// UseAuthorization() = middleware untuk authorization/authentication
// Meskipun belum implement auth, disimpan untuk future implementation
app.UseAuthorization();

// MapControllers() = mapping route ke controller actions
// Tanpa ini, routing ke controller tidak akan berfungsi
app.MapControllers();

// =====================================
// START APPLICATION
// =====================================
// Run() = start web server dan listen untuk incoming requests
// Method ini blocking - aplikasi akan terus berjalan sampai di-stop
app.Run();

// =====================================
// PENJELASAN FLOW APLIKASI:
// =====================================
// 1. Builder pattern untuk konfigurasi services dan dependencies
// 2. Dependency Injection container akan create instance sesuai kebutuhan
// 3. HTTP pipeline akan process setiap request melalui middleware
// 4. Router akan direct request ke controller yang sesuai
// 5. Controller akan call repository untuk data access
// 6. Repository akan connect ke database dan return data
// 7. Response akan dikirim kembali ke client
