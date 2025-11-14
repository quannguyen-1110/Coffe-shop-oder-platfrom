using Amazon.DynamoDBv2;
using Amazon.LocationService;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Services;
using CoffeeShopAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Amazon.DynamoDBv2.DataModel;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// === AWS Services ===
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddAWSService<IAmazonLocationService>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();

// Initialize DynamoDB tables
builder.Services.AddSingleton<DynamoDbService>();


// === App services ===
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ShipperAuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<LoyaltyService>();
builder.Services.AddScoped<OrderItemService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ShippingService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<DrinkRepository>();
builder.Services.AddScoped<CakeRepository>();
builder.Services.AddScoped<ToppingRepository>();
builder.Services.AddScoped<VNPayService>();
builder.Services.AddHttpClient<MoMoService>();
builder.Services.AddScoped<ShipperProfileRepository>();
builder.Services.AddScoped<ShipperDeliveryHistoryRepository>();

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000",
                "https://main.d3djm3hylbiyyu.amplifyapp.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// === JWT Authentication (Hybrid: Cognito + Local) ===
var region = builder.Configuration["AWS:Region"];
var userPoolId = builder.Configuration["Cognito:UserPoolId"];
var clientId = builder.Configuration["Cognito:ClientId"];
var shipperJwtKey = builder.Configuration["Jwt:LocalKey"];

if (string.IsNullOrEmpty(shipperJwtKey))
{
    throw new Exception("Missing Jwt:LocalKey in appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        // Cognito JWT cho User/Admin
        options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = clientId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = "custom:role",
            NameClaimType = "cognito:username"
        };
    })
    .AddJwtBearer("ShipperAuth", options =>
    {
        // Local JWT cho Shipper
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(shipperJwtKey)),
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        };
    });

// Cho phép cả 2 loại JWT
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "ShipperAuth")
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// === Swagger with JWT Bearer Auth ===
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CoffeeShopAPI",
        Version = "v1"
    });

    // ⚙️ Định nghĩa đúng kiểu JWT Bearer để Swagger tự thêm prefix
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http, // 👈 PHẢI LÀ Http, KHÔNG PHẢI ApiKey
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập ID token từ AWS Cognito (KHÔNG cần gõ chữ 'Bearer')"
    });

    // ⚙️ Bắt Swagger gửi token trong tất cả request
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});


// === Build app ===
var app = builder.Build();

// Ensure DynamoDB tables are created
var dynamoDbService = app.Services.GetRequiredService<DynamoDbService>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
