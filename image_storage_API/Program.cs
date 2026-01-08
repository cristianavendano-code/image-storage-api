using image_storage_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Registrar servicios en DI Container
//builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========== CONFIGURAR AUTENTICACIÓN JWT ==========
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret no configurado en appsettings.json");

builder.Services.AddAuthentication(options =>
{
    // Usar JWT Bearer como esquema por defecto
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validar que el token tenga el Issuer correcto
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        // Validar que el token tenga el Audience correcto
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        // Validar que el token no haya expirado
        ValidateLifetime = true,

        // Validar la firma del token
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret)
            )
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ========== MIDDLEWARE DE AUTENTICACIÓN ==========
// ORDEN IMPORTANTE: Authentication ANTES de Authorization
app.UseAuthentication();  // ← Lee el token y valida
app.UseAuthorization();  // ← Verifica permisos

app.MapControllers();

app.Run();
