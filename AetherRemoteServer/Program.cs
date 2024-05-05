using AetherRemoteServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);

builder.WebHost.UseUrls("http://10.0.0.148:25565");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<MainHub>("/mainHub");
app.MapHub<AdminHub>("/adminHub");

app.Run();
