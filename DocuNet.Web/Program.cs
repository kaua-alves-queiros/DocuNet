using DocuNet.Web.Components;
using DocuNet.Web.Constants;
using DocuNet.Web.Data;
using DocuNet.Web.Models;
using DocuNet.Web.Services;
using DocuNet.Web.Extensions;
using DocuNet.Web.States;
using Microsoft.AspNetCore.Identity;
using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Mvc;

namespace DocuNet.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddMudServices();
            builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services));
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDatabaseContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(SystemRoles.SystemAdministrator, policy => policy.RequireRole(SystemRoles.SystemAdministrator));
            });

            builder.Services.AddDbContext<ApplicationDatabaseContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Database")));

            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<OrganizationService>();
            builder.Services.AddScoped<DeviceService>();
            builder.Services.AddScoped<OrganizationState>();
            var app = builder.Build();

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

            app.MapPost("/account/login", async (
                [FromForm] string email, 
                [FromForm] string password, 
                [FromForm] string? returnUrl,
                SignInManager<User> signInManager) =>
            {
                var result = await signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Garante que o redirecionamento seja para uma URL local e não vazia
                    if (string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith("/"))
                    {
                        return Results.Redirect("/");
                    }
                    return Results.Redirect(returnUrl);
                }
                
                var errorUrl = "/account/login?error=InvalidLogin";
                if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/"))
                {
                    errorUrl += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
                }
                return Results.Redirect(errorUrl);
            }).DisableAntiforgery();

            app.MapGet("/account/logout", async (
                SignInManager<User> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return Results.Redirect("/");
            }).ExcludeFromDescription();

            await app.InitializeDatabaseAsync();

            await app.RunAsync();
        }
    }
}
