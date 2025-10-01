// Program.cs
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;                 // SameSiteMode / CookieSecurePolicy
using Microsoft.Extensions.Hosting;              // IHostEnvironment
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;             // UseOracle
using MottuProjeto.Data;                         // AppDbContext
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger + XML comments
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MottuProjeto API",
        Version = "v1",
        Description = "API de gestão de usuários, motos e áreas (Sprint 1)."
    });

    // Lê os comentários XML gerados pelo .csproj para descrever endpoints, params e models
    var xml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

// DbContext (Oracle) — Connection string via appsettings ou env
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var conn = Environment.GetEnvironmentVariable("ORACLE_CONN")
              ?? builder.Configuration.GetConnectionString("Default");

    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException(
            "Defina ORACLE_CONN ou ConnectionStrings:Default no appsettings.json");

    opt.UseOracle(conn);
});

// Autenticação via Cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AuthCookie";
        options.LoginPath = "/api/auth/login";
        options.AccessDeniedPath = "/api/auth/denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;            // ok para HTTP local
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // em produção (HTTPS), use Always
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger SEMPRE habilitado (independente do ambiente)
app.UseSwagger();
app.UseSwaggerUI();

// Opcional: comente se não tiver HTTPS configurado para evitar warning
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SEED opcional de admin (apenas se a base estiver vazia)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Database.EnsureCreated(); // Cria o banco se não existir
        
        if (!ctx.Usuarios.Any())
        {
            ctx.Usuarios.Add(new MottuProjeto.Models.Usuario
            {
                Nome = "Administrador",
                Email = "admin@example.com",
                Username = "admin",
                PasswordHash = "admin123", // ⚠️ Em produção, usar HASH real (BCrypt/Argon2)
                Role = "Admin"
            });
            ctx.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro no seed: {ex.Message}");
    }
}

app.Run();
