// using Microsoft.EntityFrameworkCore;
using BE.Data;
using BE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using BE.Models; // thêm namespace để dùng User model

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.CaptureStartupErrors(true);
builder.WebHost.UseSetting("detailedErrors", "true");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đọc config MongoDB
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDbService>();

// Đăng ký các service
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<JwtService>();

// Cấu hình JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware xử lý JWT
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// ===================== SEED ADMIN ACCOUNT =====================
using (var scope = app.Services.CreateScope())
{
    var mongoService = scope.ServiceProvider.GetRequiredService<MongoDbService>();
    var users = mongoService.GetCollection<User>("Users");

    var existingAdmin = await users.Find(u => u.Username == "admin").FirstOrDefaultAsync();
    if (existingAdmin == null)
    {
        using var sha = SHA256.Create();
        var passwordBytes = Encoding.UTF8.GetBytes("admin123");
        var hashedPassword = Convert.ToBase64String(sha.ComputeHash(passwordBytes));

        var adminUser = new User
        {
            Username = "admin",
            Password = hashedPassword,
            Role = "admin"
        };

        await users.InsertOneAsync(adminUser);
        Console.WriteLine(" Admin account created: username=admin, password=admin123");
    }
    else
    {
        Console.WriteLine(" Admin account already exists.");
    }
}
// ===============================================================

app.Run();
