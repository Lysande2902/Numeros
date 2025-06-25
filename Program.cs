using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ParImparAPI.Domain.Data;
using ParImparAPI.Domain.Entities;
using ParImparAPI.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ParImparAPI", Version = "v1" });

    // Configurar Swagger para JWT
    c.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        }
    );

    c.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        }
    );
});

// Configuración de CORS (si es necesario)
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

// Configuración de EF Core con MySQL (Railway) - Proveedor oficial
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(
        "Server=metro.proxy.rlwy.net;Port=45142;Database=railway;Uid=root;Pwd=qeOmVybzwXZURCcGrVZHgNzThXeQlUiU;"
    )
);

// Registrar el servicio de autenticación
builder.Services.AddScoped<AuthService>();

// Configuración de JWT
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ParImparAPI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ParImparAPI",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKey12345678901234567890"
                )
            ),
        };
    });

// Crear la aplicación
var app = builder.Build();

// Usar Swagger en desarrollo y producción (útil para Somee)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParImparAPI v1");
    c.RoutePrefix = "swagger";
});

// Configurar middleware
// Removido UseHttpsRedirection() para Somee

// Usar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Usar CORS (si es necesario)
app.UseCors("AllowAll");

// Mapear controladores
app.MapControllers();

// Ejecutar la aplicación
app.Run();
