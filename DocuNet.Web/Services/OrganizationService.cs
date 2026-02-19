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

            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                var errors = string.Join(" ", validationResults.Select(r => r.ErrorMessage));
                return new ServiceResultDto<Guid>(false, Guid.Empty, $"Dados inválidos: {errors}");
            }

            var requester = await userManager.FindByIdAsync(dto.CreatedBy.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator) || await userManager.IsLockedOutAsync(requester))
            {
                logger.LogWarning("Tentativa de criar organização negada: Solicitante {CreatedBy} não tem permissão ou está inativo.", dto.CreatedBy);
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Acesso negado: Você não tem permissão para criar organizações.");
            }

            if (await context.Organizations.AnyAsync(o => o.Name.ToLower() == dto.Name.ToLower()))
            {
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Já existe uma organização com este nome.");
            }

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

        /// <summary>
        /// Altera o nome de uma organização existente.
        /// Operação restrita a administradores ativos e organizações ativas.
        /// </summary>
        /// <param name="dto">Dados para o renome da organização.</param>
        /// <returns>Verdadeiro se a organização foi renomeada com sucesso.</returns>
        public async Task<ServiceResultDto<bool>> RenameOrganizationAsync(RenameOrganizationDto dto)
        {
            logger.LogInformation("Tentando renomear organização {Id} para {NewName} por {RequesterId}", dto.OrganizationId, dto.NewName, dto.RequesterId);

            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                var errors = string.Join(" ", validationResults.Select(r => r.ErrorMessage));
                return new ServiceResultDto<bool>(false, false, $"Dados inválidos: {errors}");
            }

            var requester = await userManager.FindByIdAsync(dto.RequesterId.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator) || await userManager.IsLockedOutAsync(requester))
            {
                logger.LogWarning("Tentativa de renomear organização negada: Solicitante {RequesterId} não tem permissão ou está inativo.", dto.RequesterId);
                return new ServiceResultDto<bool>(false, false, "Acesso negado: Você não tem permissão para gerenciar organizações.");
            }

            var organization = await context.Organizations.FindAsync(dto.OrganizationId);
            if (organization == null)
            {
                return new ServiceResultDto<bool>(false, false, "Organização não encontrada.");
            }

            if (!organization.IsActive)
            {
                logger.LogWarning("Tentativa de renomear organização inativa {Id}.", dto.OrganizationId);
                return new ServiceResultDto<bool>(false, false, "Acesso negado: Não é possível renomear uma organização desativada.");
            }

            if (await context.Organizations.AnyAsync(o => o.Id != dto.OrganizationId && o.Name.ToLower() == dto.NewName.ToLower()))
            {
                return new ServiceResultDto<bool>(false, false, "Já existe outra organização com este nome.");
            }

            organization.Name = dto.NewName;

            try
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Organização {Id} renomeada para {NewName} com sucesso.", dto.OrganizationId, dto.NewName);
                return new ServiceResultDto<bool>(true, true, "Organização renomeada com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao atualizar nome da organização {Id} no banco de dados.", dto.OrganizationId);
                return new ServiceResultDto<bool>(false, false, "Erro interno ao atualizar a organização.");
            }
        }

        /// <summary>
        /// Gerencia o status de ativação/desativação de uma organização.
        /// Operação restrita a administradores ativos.
        /// </summary>
        /// <param name="dto">Dados para alteração de status.</param>
        /// <returns>Verdadeiro se o status foi alterado com sucesso.</returns>
        public async Task<ServiceResultDto<bool>> ManageOrganizationStatusAsync(ManageOrganizationStatusDto dto)
        {
            logger.LogInformation("Tentando alterar status da organização {Id} para Enabled={IsEnabled} por {RequesterId}", dto.OrganizationId, dto.IsEnabled, dto.RequesterId);

            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                var errors = string.Join(" ", validationResults.Select(r => r.ErrorMessage));
                return new ServiceResultDto<bool>(false, false, $"Dados inválidos: {errors}");
            }

            var requester = await userManager.FindByIdAsync(dto.RequesterId.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator) || await userManager.IsLockedOutAsync(requester))
            {
                logger.LogWarning("Permissão negada ou conta inativa ao tentar gerenciar status da organização.");
                return new ServiceResultDto<bool>(false, false, "Você não tem permissão ou sua conta está desativada.");
            }

            var organization = await context.Organizations.FindAsync(dto.OrganizationId);
            if (organization == null)
            {
                return new ServiceResultDto<bool>(false, false, "Organização não encontrada.");
            }

            organization.IsActive = dto.IsEnabled;

            try
            {
                await context.SaveChangesAsync();
                var action = dto.IsEnabled ? "habilitada" : "desabilitada";
                logger.LogInformation("Organização {Id} {Action} com sucesso.", dto.OrganizationId, action);
                return new ServiceResultDto<bool>(true, true, $"Organização {action} com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao alterar status da organização {Id} no banco de dados.", dto.OrganizationId);
                return new ServiceResultDto<bool>(false, false, "Erro interno ao gerenciar status da organização.");
            }
        }
    }
}
