using System.Net;
using System.Text;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
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
        if (Configuration.Load() is not { } configuration)
        {
            Environment.Exit(1);
            return;
        }
        
        // Create service builder
        var builder = WebApplication.CreateBuilder(args);

        // Configuration Authentication and Authorization
        ConfigureJwtAuthentication(builder.Services, configuration);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddSignalR(options => options.EnableDetailedErrors = true)
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = MessagePackSerializerOptions.Standard
                    .WithSecurity(MessagePackSecurity.UntrustedData);
            });
        builder.Services.AddSingleton(configuration);

        // Services
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<IConnectionsService, ConnectionsService>();
        
        // Managers
        builder.Services.AddSingleton<IForwardedRequestManager, ForwardedRequestManager>();

        // Handles
        builder.Services.AddSingleton<OnlineStatusUpdateHandler>();
        builder.Services.AddSingleton<AddFriendHandler>();
        builder.Services.AddSingleton<BodySwapHandler>();
        builder.Services.AddSingleton<CustomizePlusHandler>();
        builder.Services.AddSingleton<EmoteHandler>();
        builder.Services.AddSingleton<GetAccountDataHandler>();
        builder.Services.AddSingleton<HypnosisHandler>();
        builder.Services.AddSingleton<MoodlesHandler>();
        builder.Services.AddSingleton<RemoveFriendHandler>();
        builder.Services.AddSingleton<SpeakHandler>();
        builder.Services.AddSingleton<TransformHandler>();
        builder.Services.AddSingleton<TwinningHandler>();
        builder.Services.AddSingleton<UpdateFriendHandler>();

#if DEBUG
        builder.WebHost.UseUrls("https://localhost:5006");
        /*
        builder.WebHost.ConfigureKestrel(options =>
        {
            var ip = IPAddress.Parse("192.168.1.14");
            options.Listen(ip, 5007, listenOptions =>
            {
                listenOptions.UseHttps($"{configuration.CertificatePath}", $"{configuration.CertificatePasswordPath}");
            });
        });
        */
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