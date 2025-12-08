using Infrastructure.Data;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using WebAPI.Seed;
using Domain.Interfaces;
using Infrastructure.Data.Repositories;
using Application.Services;
using Infrastructure.Services;
using QuestPDF.Infrastructure;

// Configurar licencia de QuestPDF (Community para uso no comercial o evaluación)
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        var json = o.JsonSerializerOptions;
        json.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        json.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Agiles Facturacion API", Version = "v1" });
    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Ingrese 'Bearer {token}'",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// Configuración de DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5241",
                    "https://localhost:5241"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// Configuración de JWT Authentication
var jwtCfg = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtCfg["Issuer"],
            ValidAudience = jwtCfg["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtCfg["Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Facturación: repositorios y servicios
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<IDetalleFacturaRepository, DetalleFacturaRepository>();
builder.Services.AddScoped<FacturaService>();
builder.Services.AddScoped<ReporteService>();
// Product lookup (obtiene precio desde la BD)
builder.Services.AddScoped<Application.Interfaces.IProductLookup, Infrastructure.Services.ProductLookup>();
// Lote allocator (asigna lotes por FIFO y decrementa stock)
builder.Services.AddScoped<Application.Interfaces.ILoteAllocator, Infrastructure.Services.LoteAllocator>();
builder.Services.Configure<Infrastructure.Services.Sri.SriOptions>(
    builder.Configuration.GetSection("Sri"));
builder.Services.AddSingleton<Infrastructure.Services.Sri.ClaveAccesoGenerator>();
builder.Services.AddSingleton<Infrastructure.Services.Sri.FacturaXmlBuilder>();
builder.Services.AddSingleton<Infrastructure.Services.Sri.XadesSigner>();
builder.Services.AddSingleton<Infrastructure.Services.Sri.SriSoapClient>();
builder.Services.AddScoped<Infrastructure.Services.Sri.SriIntegrationService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Use(async (ctx, next) =>
{
    try {
        await next();
    } catch (Exception ex) {
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsJsonAsync(new { error = "Error interno", trace = ex.Message });
    }
});

// Seed admin
await DbInitializer.InitializeAsync(app.Services);
app.Run();