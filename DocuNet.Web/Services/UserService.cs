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

            if (!await userManager.IsInRoleAsync(userRequester, SystemRoles.SystemAdministrator))
            {
                logger.LogWarning("Usuário {CreatedBy} tentou criar o usuário {Email} sem permissão de administrador.", dto.CreatedBy, dto.Email);
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Você não tem permissão para criar usuários.");
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
    }
}
