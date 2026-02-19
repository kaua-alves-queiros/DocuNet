using DocuNet.Web.Constants;
using DocuNet.Web.Data;
using DocuNet.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocuNet.Web.Extensions
{
    public static class HostExtensions
    {
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();
            var context = services.GetRequiredService<ApplicationDatabaseContext>();

            try
            {
                logger.LogInformation("Iniciando migrações de banco de dados...");
                await context.Database.MigrateAsync();

                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                // Critério de Primeiro Setup: Se não existem usuários no sistema.
                if (!await userManager.Users.AnyAsync())
                {
                    logger.LogInformation("Primeiro setup detectado. Iniciando configuração inicial...");
                    await FirstSetupAsync(userManager, roleManager, logger);
                }
                else
                {
                    logger.LogInformation("O banco de dados já possui usuários. Pulando primeiro setup.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocorreu um erro ao inicializar o banco de dados.");
                throw; // Re-throw para impedir que o app suba com estado inválido em produção
            }
        }

        private static async Task FirstSetupAsync(
            UserManager<User> userManager, 
            RoleManager<IdentityRole<Guid>> roleManager,
            ILogger logger)
        {
            // 1. Garantir que a Role de Administrador existe
            if (!await roleManager.RoleExistsAsync(SystemRoles.SystemAdministrator))
            {
                logger.LogInformation("Criando role de administrador: {Role}", SystemRoles.SystemAdministrator);
                var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(SystemRoles.SystemAdministrator));
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new Exception($"Falha ao criar role de administrador: {errors}");
                }
            }

            // 2. Criar usuário padrão
            var adminUser = await userManager.FindByEmailAsync(DefaultUser.Email);
            if (adminUser is null)
            {
                logger.LogInformation("Criando usuário administrador padrão: {Email}", DefaultUser.Email);
                adminUser = new User 
                { 
                    Email = DefaultUser.Email, 
                    UserName = DefaultUser.Email, 
                    EmailConfirmed = true 
                };

                var createResult = await userManager.CreateAsync(adminUser, DefaultUser.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"Falha ao criar usuário administrador padrão: {errors}");
                }
            }

            // 3. Vincular usuário à Role
            if (!await userManager.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator))
            {
                logger.LogInformation("Adicionando usuário padrão à role de administrador.");
                await userManager.AddToRoleAsync(adminUser, SystemRoles.SystemAdministrator);
                await userManager.UpdateSecurityStampAsync(adminUser);
            }
            
            logger.LogInformation("Primeiro setup concluído com sucesso.");
        }
    }
}
