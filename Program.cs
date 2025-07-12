using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using project_graduation.Middlewares;
using project_graduation.Model;
using project_graduation.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// CORS configuration
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()      // Allow requests from any domain
            .AllowAnyMethod()      // Allow all HTTP methods: GET, POST, PUT, DELETE, etc.
            .AllowAnyHeader();     // Allow any headers including Authorization
    });
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework and SQL Server
builder.Services.AddDbContext<appDBcontext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,                                 // Validate the token issuer
            ValidateAudience = true,                               // Validate the token audience
            ValidateLifetime = true,                               // Validate token expiration
            ValidateIssuerSigningKey = true,                       // Validate the signing key
            ValidIssuer = builder.Configuration["Jwt:Issuer"],     // Expected issuer
            ValidAudience = builder.Configuration["Jwt:Audience"], // Expected audience
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )                                                      // Signing key
        };
    });

// Register application services
builder.Services.AddScoped<EmailService>();
builder.Services.AddAuthorization();


var app = builder.Build();

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI();

// Middleware pipeline order is important
app.UseRouting();
app.UseCors("AllowAll");
app.UseRateLimitPerIp();
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes
app.MapControllers();

// Run the application
app.Run();
