using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Constants;
using DocuNet.Web.Data;
using DocuNet.Web.Dtos;
using DocuNet.Web.Dtos.Organization;
using DocuNet.Web.Dtos.User;
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

        /// <summary>
        /// Adiciona um usuário a uma organização.
        /// Operação restrita a administradores ativos, organizações ativas e usuários ativos.
        /// </summary>
        /// <param name="dto">Dados da vinculação.</param>
        /// <returns>Verdadeiro se o usuário foi adicionado com sucesso.</returns>
        public async Task<ServiceResultDto<bool>> AddUserToOrganizationAsync(ManageUserInOrganizationDto dto)
        {
            logger.LogInformation("Tentando adicionar usuário {UserId} à organização {OrgId} por {RequesterId}", dto.UserId, dto.OrganizationId, dto.RequesterId);

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
                return new ServiceResultDto<bool>(false, false, "Acesso negado: Você não tem permissão ou sua conta está inativa.");
            }

            var organization = await context.Organizations.Include(o => o.Users).FirstOrDefaultAsync(o => o.Id == dto.OrganizationId);
            if (organization == null) return new ServiceResultDto<bool>(false, false, "Organização não encontrada.");
            if (!organization.IsActive) return new ServiceResultDto<bool>(false, false, "Acesso negado: A organização está desativada.");

            var user = await userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null) return new ServiceResultDto<bool>(false, false, "Usuário não encontrado.");
            if (await userManager.IsLockedOutAsync(user)) return new ServiceResultDto<bool>(false, false, "Acesso negado: Não é possível adicionar um usuário desativado.");

            if (organization.Users.Any(u => u.Id == dto.UserId))
            {
                return new ServiceResultDto<bool>(false, false, "O usuário já faz parte desta organização.");
            }

            organization.Users.Add(user);
            try
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Usuário {UserId} adicionado à organização {OrgId} com sucesso.", dto.UserId, dto.OrganizationId);
                return new ServiceResultDto<bool>(true, true, "Usuário adicionado com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao adicionar usuário à organização.");
                return new ServiceResultDto<bool>(false, false, "Erro interno ao processar a operação.");
            }
        }

        /// <summary>
        /// Remove um usuário de uma organização.
        /// Operação restrita a administradores ativos e organizações ativas.
        /// </summary>
        /// <param name="dto">Dados da desvinculação.</param>
        /// <returns>Verdadeiro se o usuário foi removido com sucesso.</returns>
        public async Task<ServiceResultDto<bool>> RemoveUserFromOrganizationAsync(ManageUserInOrganizationDto dto)
        {
            logger.LogInformation("Tentando remover usuário {UserId} da organização {OrgId} por {RequesterId}", dto.UserId, dto.OrganizationId, dto.RequesterId);

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
                return new ServiceResultDto<bool>(false, false, "Acesso negado: Você não tem permissão ou sua conta está inativa.");
            }

            var organization = await context.Organizations.Include(o => o.Users).FirstOrDefaultAsync(o => o.Id == dto.OrganizationId);
            if (organization == null) return new ServiceResultDto<bool>(false, false, "Organização não encontrada.");
            if (!organization.IsActive) return new ServiceResultDto<bool>(false, false, "Acesso negado: A organização está desativada.");

            var user = organization.Users.FirstOrDefault(u => u.Id == dto.UserId);
            if (user == null)
            {
                return new ServiceResultDto<bool>(false, false, "O usuário não pertence a esta organização.");
            }

            organization.Users.Remove(user);
            try
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Usuário {UserId} removido da organização {OrgId} com sucesso.", dto.UserId, dto.OrganizationId);
                return new ServiceResultDto<bool>(true, true, "Usuário removido com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao remover usuário da organização.");
                return new ServiceResultDto<bool>(false, false, "Erro interno ao processar a operação.");
            }
        }

        /// <summary>
        /// Obtém uma lista resumida de todas as organizações.
        /// </summary>
        public async Task<ServiceResultDto<List<OrganizationSummaryDto>>> GetAllOrganizationsAsync(Guid requesterId)
        {
            var requester = await userManager.FindByIdAsync(requesterId.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator) || await userManager.IsLockedOutAsync(requester))
            {
                return new ServiceResultDto<List<OrganizationSummaryDto>>(false, [], "Acesso negado.");
            }

            var organizations = await context.Organizations.Include(o => o.Users).ToListAsync();
            var summaries = organizations.Select(o => new OrganizationSummaryDto(
                o.Id,
                o.Name,
                o.IsActive,
                o.Users.Count
            )).ToList();

            return new ServiceResultDto<List<OrganizationSummaryDto>>(true, summaries, "Organizações carregadas com sucesso.");
        }

        /// <summary>
        /// Obtém a lista de membros de uma organização.
        /// </summary>
        public async Task<ServiceResultDto<List<UserSummaryDto>>> GetOrganizationMembersAsync(Guid requesterId, Guid organizationId)
        {
            var requester = await userManager.FindByIdAsync(requesterId.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator) || await userManager.IsLockedOutAsync(requester))
            {
                return new ServiceResultDto<List<UserSummaryDto>>(false, [], "Acesso negado.");
            }

            var organization = await context.Organizations.Include(o => o.Users).FirstOrDefaultAsync(o => o.Id == organizationId);
            if (organization == null)
            {
                return new ServiceResultDto<List<UserSummaryDto>>(false, [], "Organização não encontrada.");
            }

            var summaries = new List<UserSummaryDto>();
            foreach (var user in organization.Users)
            {
                var roles = (await userManager.GetRolesAsync(user)).ToList();
                var isLockedOut = await userManager.IsLockedOutAsync(user);
                summaries.Add(new UserSummaryDto(user.Id, user.Email!, isLockedOut, roles));
            }

            return new ServiceResultDto<List<UserSummaryDto>>(true, summaries, "Membros carregados com sucesso.");
        }
        /// <summary>
        /// Obtém a lista de organizações disponíveis para o usuário (Todas se Admin, apenas as vinculadas se Membro).
        /// </summary>
        public async Task<ServiceResultDto<List<OrganizationSummaryDto>>> GetAvailableOrganizationsAsync(Guid requesterId)
        {
            var requester = await userManager.FindByIdAsync(requesterId.ToString());
            if (requester == null || await userManager.IsLockedOutAsync(requester))
            {
                return new ServiceResultDto<List<OrganizationSummaryDto>>(false, [], "Acesso negado ou conta inativa.");
            }

            List<Organization> organizations;

            if (await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator))
            {
                // Admin vê todas
                organizations = await context.Organizations.Include(o => o.Users).ToListAsync();
            }
            else
            {
                // Membro vê apenas as suas
                organizations = await context.Organizations
                    .Include(o => o.Users)
                    .Where(o => o.Users.Any(u => u.Id == requesterId))
                    .ToListAsync();
            }

            var summaries = organizations.Select(o => new OrganizationSummaryDto(
                o.Id,
                o.Name,
                o.IsActive,
                o.Users.Count
            )).ToList();

            return new ServiceResultDto<List<OrganizationSummaryDto>>(true, summaries, "Organizações carregadas com sucesso.");
        }
    }
}
