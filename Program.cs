using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pic.Classes;
using Pic.Config;
using Pic.Interface;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//builder.Services.AddScoped<EmailVerify>();
builder.Services.AddSingleton<Token>();
builder.Services.AddSingleton<IPasswordHasher<Users>, PasswordHasher<Users>>();
builder.Services.AddSingleton<PasswordHash>();
builder.Services.AddScoped<IEnviar,Enviar>();
builder.Services.AddScoped<IUsers, Users>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContextPool<Pic.Context.AppDbContext>(options => 
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
poolSize: 20);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer("Usuarios", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Bearer:Issuer"],
                ValidAudience = builder.Configuration["Bearer:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Bearer:Key"]))
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            return context.Response.WriteAsync("{\"Mensagem\": \"Token expirado\"}");
                        }
                        else
                        {
                            return context.Response.WriteAsync("{\"Mensagem\": \"Token inválido\"}");
                        }
                    }
                    return Task.CompletedTask;
                },

                OnForbidden = context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"Mensagem\": \"Você não tem permissão para fazer isso\"}");
                    }
                    return Task.CompletedTask;
                },

                OnChallenge = context =>
                {
                    context.HandleResponse();
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"Mensagem\": \"Nenhum Token enviado\"}");
                    }
                    return Task.CompletedTask;
                },
            };

        });

builder.Services.AddRateLimiter(p =>
{
    p.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsync("{\"Mensagem\": \"Muitas requisições, por favor tente mais tarde.\"}", token);
    };

    p.AddFixedWindowLimiter("Fixed", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromSeconds(10);
        options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });

});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<Pic.Middleware.PrimeiroMiddleware>();
app.MapControllers();

app.Run();
