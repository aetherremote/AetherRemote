using System.Security.Cryptography.X509Certificates;
using System.Text;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Kestrel;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.Services.Database;
using AetherRemoteServer.SignalR.Handlers;
using AetherRemoteServer.SignalR.Hubs;
using MessagePack;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AetherRemoteServer;

// ReSharper disable once ClassNeverInstantiated.Global

public class Program
{
    private static void Main(string[] args)
    {
        // Attempt to load configuration values
        if (ConfigurationService.Load() is not { } configuration)
        {
            Environment.Exit(1);
            return;
        }
        
        // Create service builder
        var builder = WebApplication.CreateBuilder(args);

        // Configuration Authentication and Authorization
        ConfigureJwtAuthentication(builder.Services, configuration);
        
        // Configure Kestrel based on environment
        ConfigureKestrel(builder, configuration);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddSignalR(options => options.EnableDetailedErrors = true)
            .AddMessagePackProtocol(options => options.SerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData));
        builder.Services.AddSingleton(configuration);

        // Services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<PresenceService>();
        builder.Services.AddSingleton<RequestLoggingService>();
        
        // Managers
        builder.Services.AddSingleton<ForwardedRequestManager>();
        builder.Services.AddSingleton<PossessionManager>();

        // Handles
        builder.Services.AddSingleton<RequestHandler>();

        // Finalize
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
    
    private static void ConfigureKestrel(WebApplicationBuilder builder, Configuration configuration)
    {
        if (builder.Configuration.GetSection("Kestrel").Get<KestrelConfigurations>() is not { } configurations)
            return;
        
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(configurations.Port, listenOptions =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    listenOptions.UseHttps();
                }
                else
                {
                    var certificate = X509Certificate2.CreateFromPemFile(
                        configuration.CertificateCrtPath, 
                        configuration.CertificateKeyPath
                    );
                
                    listenOptions.UseHttps(certificate);
                }
            });
        });
    }

    private static void ConfigureJwtAuthentication(IServiceCollection services, Configuration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.SigningKey)),
            };
        });
    }
}