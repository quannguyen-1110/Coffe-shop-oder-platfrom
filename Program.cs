using Amazon.DynamoDBv2;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// === AWS DynamoDB ===
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonDynamoDB>();

// === App services ===
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<VoucherRepository>();
builder.Services.AddScoped<OrderRepository>();

// === CORS: Cho phép tất cả (Frontend truy cập được) ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// === JWT Authentication (Cognito) ===
var cognitoUserPoolId = builder.Configuration["Cognito:UserPoolId"];
var region = builder.Configuration["AWS:Region"];

if (string.IsNullOrEmpty(cognitoUserPoolId) || string.IsNullOrEmpty(region))
{
    throw new Exception(" Missing Cognito UserPoolId or AWS Region in appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{cognitoUserPoolId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false, // set true nếu bạn cần khớp clientId
            ValidateLifetime = true,
            RoleClaimType = "custom:role" // Dòng này giúp .NET đọc claim custom:role làm Role
        };
    });


// === Authorization & MVC ===
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === Build app ===
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// CORS trước authentication
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
