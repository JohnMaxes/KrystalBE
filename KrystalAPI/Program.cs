
using KrystalAPI.Data;
using KrystalAPI.Models;
using KrystalAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace KrystalAPI
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "KrystalAPI",
                    Version = "v1",
                    Description = "KrystalAPI documentation, empowered with OpenAPI",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "BaoDang",
                        Email = "baodang9855.tdn@gmail.com",
                        Url = new Uri("https://github.com/JohnMaxes/KrystalBE")
                    }
                });
            });

            builder.Services.AddDbContext<UserContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
            );

            builder.Services.AddSingleton<JwtService>();

            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                var secret = jwtSettings!.Secret;
                var issuer = jwtSettings!.Issuer;
                var audience = jwtSettings!.Audience;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    RoleClaimType = "role",
                    NameClaimType = JwtRegisteredClaimNames.Sub
                };
            });

            var app = builder.Build();

            app.UseStaticFiles();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.MapOpenApi();
                // app.UseSwagger(); // automatic swagger.json generation
                app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/swagger.json", "Static API Docs");
                        c.RoutePrefix = "docs";
                    }
                );  // Static file mapping and serving
            }
            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
