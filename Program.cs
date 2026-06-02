using JwtDemo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ─── JWT Authentication ─────────────────────────────────────────────────────
var jwtConfig = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.UTF8.GetBytes(jwtConfig["Secret"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtConfig["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtConfig["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),

            // Kiểm tra thời gian hết hạn của access token
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // Không cho phép sai lệch đồng hồ
        };
    });

builder.Services.AddAuthorization();

// ─── Application Services ───────────────────────────────────────────────────
// Singleton vì UserService giữ state in-memory
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IJwtService, JwtService>();

// ─── Swagger (tùy chọn) ─────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "JwtDemo API", Version = "v1" });

    // Thêm nút "Authorize" trong Swagger UI để nhập Bearer token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập access token. Ví dụ: eyJhbGci...",
    });

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
            []
        }
    });
});

var app = builder.Build();

// ─── Middleware Pipeline ─────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Thứ tự quan trọng: Authentication TRƯỚC Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
