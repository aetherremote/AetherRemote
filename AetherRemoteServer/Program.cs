using System.Security.Cryptography.X509Certificates;
using System.Text;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Kestrel;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.SignalR.Handlers;
using AetherRemoteServer.SignalR.Hubs;
using MessagePack;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DatabaseInfrastructure = AetherRemoteServer.Infrastructure.Database.DatabaseInfrastructure;

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

        // Infrastructure
        builder.Services.AddSingleton<DatabaseInfrastructure>();
        
        // Services
        builder.Services.AddSingleton<PermissionsService>();
        builder.Services.AddSingleton<RequestLoggingService>();
        builder.Services.AddSingleton<SessionService>();
        
        // Managers
        builder.Services.AddSingleton<PossessionManager>();
        builder.Services.AddSingleton<RelayManager>();
        
        // Handler Base
        builder.Services.AddSingleton<AddFriendHandler>();
        builder.Services.AddSingleton<BodySwapHandler>();
        builder.Services.AddSingleton<CustomizePlusHandler>();
        builder.Services.AddSingleton<EmoteHandler>();
        builder.Services.AddSingleton<HonorificHandler>();
        builder.Services.AddSingleton<HypnosisHandler>();
        builder.Services.AddSingleton<HypnosisStopHandler>();
        builder.Services.AddSingleton<InitializeSessionHandler>();
        builder.Services.AddSingleton<MoodlesHandler>();
        builder.Services.AddSingleton<OnlineNotificationHandler>();
        builder.Services.AddSingleton<RemoveFriendHandler>();
        builder.Services.AddSingleton<SpeakHandler>();
        builder.Services.AddSingleton<TerminateSessionHandler>();
        builder.Services.AddSingleton<TransformationHandler>();
        builder.Services.AddSingleton<TwinningHandler>();
        builder.Services.AddSingleton<UpdateFriendHandler>();
        builder.Services.AddSingleton<UpdateGlobalPermissionsHandler>();
        
        // Handler Aggregate
        builder.Services.AddSingleton<AggregateRequestHandler>();

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