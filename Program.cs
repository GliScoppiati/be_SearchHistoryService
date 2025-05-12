using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using SearchHistoryService.Data;
using SearchHistoryService.Services;

var builder = WebApplication.CreateBuilder(args);

// 📌 Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 📌 Swagger + JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Search History API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer token",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// 📌 PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 📌 JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes   = Encoding.ASCII.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken            = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSection["Issuer"],
            ValidAudience            = jwtSection["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddAuthorization();

// 📌 CORS - per ora aperto
builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                                  .AllowAnyHeader()
                                  .AllowAnyMethod());
});

// 📌 Services
builder.Services.AddScoped<SearchHistoryRecorder>();

var app = builder.Build();

// Recupera il logger di Program
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// 🛎️ AUTO-MIGRATION E CREAZIONE DATABASE
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logCtx = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    const int maxAttempts = 10;

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            db.Database.Migrate();
            logCtx.LogInformation("[SearchHistoryService] ✅ Migration completata.");
            break;
        }
        catch when (attempt < maxAttempts)
        {
            logCtx.LogWarning(
                "[SearchHistoryService] ⏳ DB non pronto… ritento ({Attempt}/{MaxAttempts})",
                attempt,
                maxAttempts
            );
            Thread.Sleep(2000);
        }
    }
}

// 🚦 Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");   // 💡 IMPORTANTISSIMO → prima di Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 🟢 Avvio del service
logger.LogInformation("[SearchHistoryService] 🕵️‍♂️ Service avviato.");

app.Run();
