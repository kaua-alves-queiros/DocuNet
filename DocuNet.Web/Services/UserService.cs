using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Constants;
using DocuNet.Web.Dtos;
using DocuNet.Web.Dtos.User;
using DocuNet.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace DocuNet.Web.Services
{
    public class UserService(ILogger<UserService> logger, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        public async Task<ServiceResultDto<Guid>> CreateUserAsync(CreateUserDto dto)
        {
            logger.LogInformation("Iniciando criação de usuário. Email: {Email}, Criado por: {CreatedBy}", dto.Email, dto.CreatedBy);

            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                var errors = string.Join(" ", validationResults.Select(r => r.ErrorMessage));
                logger.LogWarning("Falha na validação do DTO ao criar usuário {Email}: {Errors}", dto.Email, errors);
                return new ServiceResultDto<Guid>(false, Guid.Empty, $"Dados inválidos: {errors}");
            }

            var userRequester = await userManager.FindByIdAsync(dto.CreatedBy.ToString());
            if (userRequester == null)
            {
                logger.LogWarning("Tentativa de criação de usuário frustrada: Solicitante {CreatedBy} não encontrado.", dto.CreatedBy);
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Usuário solicitante não encontrado.");
            }

            if (!await userManager.IsInRoleAsync(userRequester, SystemRoles.SystemAdministrator) || await userManager.IsLockedOutAsync(userRequester))
            {
                logger.LogWarning("Usuário {CreatedBy} tentou criar o usuário {Email} sem permissão ou com conta inativa.", dto.CreatedBy, dto.Email);
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Acesso negado: Você não tem permissão ou sua conta está desativada.");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                UserName = dto.Email,
            };

            var createResult = await userManager.CreateAsync(newUser, dto.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(" ", createResult.Errors.Select(e => e.Description));
                logger.LogError("Erro do Identity ao criar usuário {Email}: {Errors}", dto.Email, errors);
                return new ServiceResultDto<Guid>(false, Guid.Empty, $"Erro ao criar usuário: {errors}");
            }

            logger.LogInformation("Usuário {Email} (ID: {Id}) criado com sucesso por {CreatedBy}.", dto.Email, newUser.Id, dto.CreatedBy);
            return new ServiceResultDto<Guid>(true, newUser.Id, "Usuário criado com sucesso.");
        }

        public async Task<ServiceResultDto<bool>> AddToRoleAsync(ManageUserRoleDto dto)
        {
            logger.LogInformation("Tentando adicionar papel {Role} ao usuário {UserId} por {RequesterId}", dto.RoleName, dto.UserId, dto.RequesterId);

            var requester = await userManager.FindByIdAsync(dto.RequesterId.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator))
            {
                logger.LogWarning("Permissão negada ou solicitante não encontrado ao tentar adicionar papel.");
                return new ServiceResultDto<bool>(false, false, "Você não tem permissão para gerenciar papéis.");
            }

            var user = await userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
            {
                logger.LogWarning("Usuário {UserId} não encontrado para adição de papel.", dto.UserId);
                return new ServiceResultDto<bool>(false, false, "Usuário não encontrado.");
            }

            if (!await roleManager.RoleExistsAsync(dto.RoleName))
            {
                logger.LogWarning("Papel {Role} não existe no sistema.", dto.RoleName);
                return new ServiceResultDto<bool>(false, false, "O papel especificado não existe.");
            }

            var result = await userManager.AddToRoleAsync(user, dto.RoleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                logger.LogError("Erro ao adicionar papel {Role} ao usuário {UserId}: {Errors}", dto.RoleName, dto.UserId, errors);
                return new ServiceResultDto<bool>(false, false, $"Erro: {errors}");
            }

            await userManager.UpdateSecurityStampAsync(user);
            logger.LogInformation("Papel {Role} adicionado com sucesso ao usuário {UserId}.", dto.RoleName, dto.UserId);
            return new ServiceResultDto<bool>(true, true, "Papel adicionado com sucesso.");
        }

        public async Task<ServiceResultDto<bool>> RemoveFromRoleAsync(ManageUserRoleDto dto)
        {
            logger.LogInformation("Tentando remover papel {Role} do usuário {UserId} por {RequesterId}", dto.RoleName, dto.UserId, dto.RequesterId);

            var requester = await userManager.FindByIdAsync(dto.RequesterId.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator))
            {
                logger.LogWarning("Permissão negada ou solicitante não encontrado ao tentar remover papel.");
                return new ServiceResultDto<bool>(false, false, "Você não tem permissão para gerenciar papéis.");
            }

            var user = await userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
            {
                logger.LogWarning("Usuário {UserId} não encontrado para remoção de papel.", dto.UserId);
                return new ServiceResultDto<bool>(false, false, "Usuário não encontrado.");
            }

            var result = await userManager.RemoveFromRoleAsync(user, dto.RoleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                logger.LogError("Erro ao remover papel {Role} do usuário {UserId}: {Errors}", dto.RoleName, dto.UserId, errors);
                return new ServiceResultDto<bool>(false, false, $"Erro: {errors}");
            }

            await userManager.UpdateSecurityStampAsync(user);
            logger.LogInformation("Papel {Role} removido com sucesso do usuário {UserId}.", dto.RoleName, dto.UserId);
            return new ServiceResultDto<bool>(true, true, "Papel removido com sucesso.");
        }
    }
}
