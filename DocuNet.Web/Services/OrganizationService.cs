using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Constants;
using DocuNet.Web.Data;
using DocuNet.Web.Dtos;
using DocuNet.Web.Dtos.Organization;
using DocuNet.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocuNet.Web.Services
{
    /// <summary>
    /// Serviço responsável pela gestão de organizações no sistema.
    /// </summary>
    public class OrganizationService(
        ApplicationDatabaseContext context, 
        ILogger<OrganizationService> logger,
        UserManager<User> userManager)
    {
        /// <summary>
        /// Cria uma nova organização no sistema. 
        /// Operação restrita a administradores ativos.
        /// </summary>
        /// <param name="dto">Dados da organização a ser criada.</param>
        /// <returns>O ID da organização criada em caso de sucesso.</returns>
        public async Task<ServiceResultDto<Guid>> CreateOrganizationAsync(CreateOrganizationDto dto)
        {
            logger.LogInformation("Iniciando criação de organização {Name} por {CreatedBy}", dto.Name, dto.CreatedBy);

            // 1. Validação do DTO
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                var errors = string.Join(" ", validationResults.Select(r => r.ErrorMessage));
                return new ServiceResultDto<Guid>(false, Guid.Empty, $"Dados inválidos: {errors}");
            }

            // 2. Validação de Permissão (Apenas Admin Ativo)
            var requester = await userManager.FindByIdAsync(dto.CreatedBy.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator) || await userManager.IsLockedOutAsync(requester))
            {
                logger.LogWarning("Tentativa de criar organização negada: Solicitante {CreatedBy} não tem permissão ou está inativo.", dto.CreatedBy);
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Acesso negado: Você não tem permissão para criar organizações.");
            }

            // 3. Verificar se já existe uma organização com o mesmo nome
            if (await context.Organizations.AnyAsync(o => o.Name.ToLower() == dto.Name.ToLower()))
            {
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Já existe uma organização com este nome.");
            }

            // 4. Persistência
            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            try
            {
                context.Organizations.Add(organization);
                await context.SaveChangesAsync();

                logger.LogInformation("Organização {Name} (ID: {Id}) criada com sucesso.", dto.Name, organization.Id);
                return new ServiceResultDto<Guid>(true, organization.Id, "Organização criada com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao salvar organização {Name} no banco de dados.", dto.Name);
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Erro interno ao salvar a organização.");
            }
        }
    }
}
