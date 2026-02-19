using DocuNet.Web.Components;
using DocuNet.Web.Constants;
using DocuNet.Web.Data;
using DocuNet.Web.Models;
using DocuNet.Web.Services;
using DocuNet.Web.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DocuNet.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services));
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("SystemAdmin", policy => policy.RequireRole("SystemAdmin"));
            });
            builder.Services.AddDbContext<ApplicationDatabaseContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Database")));

            builder.Services.AddScoped<UserService>();
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            await app.InitializeDatabaseAsync();

            await app.RunAsync();
        }
    }
}
