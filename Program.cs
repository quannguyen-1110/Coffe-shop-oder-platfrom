using Amazon.DynamoDBv2;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Services;
using CoffeeShopAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Amazon.DynamoDBv2.DataModel;

var builder = WebApplication.CreateBuilder(args);

// === AWS DynamoDB ===
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();

// Initialize DynamoDB tables
builder.Services.AddSingleton<DynamoDbService>();


// === App services ===
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<LoyaltyService>();
builder.Services.AddScoped<OrderItemService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<DrinkRepository>();
builder.Services.AddScoped<CakeRepository>();
builder.Services.AddScoped<ToppingRepository>();
builder.Services.AddScoped<VNPayService>();

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// === JWT Authentication (AWS Cognito) ===
var region = builder.Configuration["AWS:Region"];
var userPoolId = builder.Configuration["Cognito:UserPoolId"];
var clientId = builder.Configuration["Cognito:ClientId"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = clientId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            // Cognito mappings
            RoleClaimType = "custom:role",
            NameClaimType = "cognito:username"
        };
    });

builder.Services.AddAuthorization();
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
