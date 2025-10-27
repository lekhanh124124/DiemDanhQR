// File: Program.cs
using System.Text;
using DiemDanhQR_API.Data;
using DiemDanhQR_API.Helpers;
using DiemDanhQR_API.Repositories.Interfaces;
using DiemDanhQR_API.Repositories.Implementations;
using DiemDanhQR_API.Services.Interfaces;
using DiemDanhQR_API.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ===== Serilog =====
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/app-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ===== Services =====
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký DI cho Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ILecturerRepository, LecturerRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();

// Đăng ký DI cho Service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ILecturerService, LecturerService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

// CORS: chấp nhận frontend
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowFrontend", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services
    .AddAuthentication(
        opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    )
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = false; // bật true nếu deploy HTTPS
        opt.SaveToken = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
            ValidIssuer = issuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(audience),
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name,     
            RoleClaimType = ClaimTypes.Role                        
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
    if (Directory.Exists(logDirectory)) Directory.Delete(logDirectory, true);
}

// ===== Middlewares =====
app.UseSerilogRequestLogging();
app.UseMiddleware<ApiExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
