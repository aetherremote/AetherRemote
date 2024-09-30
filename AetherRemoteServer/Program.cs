using AetherRemoteServer.Authentication.Requirements;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Hubs;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;

namespace AetherRemoteServer;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = new ServerConfiguration();
        
        // Configuration Authentication and Authorization
        ConfigureJwtAuthentication(builder.Services, configuration);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);
        builder.Services.AddSingleton(configuration);
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<NetworkService>();

#if DEBUG
        builder.WebHost.UseUrls("https://localhost:5006");
#else
        builder.WebHost.ConfigureKestrel(options =>
        {
            var ip = IPAddress.Parse("192.168.1.14");
            options.Listen(ip, 5006, listenOptions =>
            {
                listenOptions.UseHttps($"{configuration.CertificatePath}", $"{configuration.CertificatePasswordPath}");
            });
        });
#endif

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseRouting();
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHub<PrimaryHub>("/primaryHub");
        app.MapControllers();

        app.Run();
    }

    private static void ConfigureJwtAuthentication(IServiceCollection services, ServerConfiguration configuration)
    {
        services.AddTransient<IAuthorizationHandler, AdminRequirementHandler>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new()
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.SigningKey)),
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("Administrator", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new AdminRequirement());
            });
    }
}
