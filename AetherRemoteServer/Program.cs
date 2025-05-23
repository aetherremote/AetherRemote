using System.Text;
using AetherRemoteServer.Authentication.Requirements;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Authentication;
using AetherRemoteServer.Hubs;
using AetherRemoteServer.Hubs.Handlers;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using MessagePack;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace AetherRemoteServer;

// ReSharper disable once ClassNeverInstantiated.Global
public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = new Configuration();

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
        builder.Services.AddSingleton<DatabaseService>();

        // Managers
        builder.Services.AddSingleton<ConnectedClientsManager>();

        // Handles
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
        services.AddTransient<IAuthorizationHandler, AdminRequirementHandler>();

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

        services.AddAuthorizationBuilder()
            .AddPolicy("Administrator", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new AdminRequirement());
            });
    }
}