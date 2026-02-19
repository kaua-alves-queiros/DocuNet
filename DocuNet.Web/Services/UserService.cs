using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Constants;
using DocuNet.Web.Dtos;
using DocuNet.Web.Dtos.User;
using DocuNet.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocuNet.Web.Services
{
    /// <summary>
    /// Serviço responsável pela gestão de usuários no sistema.
    /// Encapsula operações de criação, gestão de papéis (roles), alteração de status (bloqueio/desbloqueio) e gestão de credenciais.
    /// </summary>
    /// <remarks>
    /// Todas as operações administrativas validam se o solicitante possui as permissões necessárias 
    /// e se sua conta não está inativa no sistema.
    /// </remarks>
    public class UserService(ILogger<UserService> logger, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        /// <summary>
        /// Cria um novo usuário no sistema.
        /// </summary>
        /// <param name="dto">Dados para criação do usuário, incluindo credenciais iniciais.</param>
        /// <returns>O ID do usuário criado em caso de sucesso.</returns>
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

        /// <summary>
        /// Atribui um papel (role) a um usuário existente.
        /// </summary>
        /// <param name="dto">Dados da atribuição, contendo o ID do usuário, a role e o solicitante.</param>
        /// <returns>Verdadeiro se a operação for concluída com sucesso.</returns>
        public async Task<ServiceResultDto<bool>> AddToRoleAsync(ManageUserRoleDto dto)
        {
            logger.LogInformation("Tentando adicionar papel {Role} ao usuário {UserId} por {RequesterId}", dto.RoleName, dto.UserId, dto.RequesterId);

            var requester = await userManager.FindByIdAsync(dto.RequesterId.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator) || await userManager.IsLockedOutAsync(requester))
            {
                logger.LogWarning("Permissão negada ou conta inativa ao tentar adicionar papel.");
                return new ServiceResultDto<bool>(false, false, "Você não tem permissão ou sua conta está desativada.");
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

        /// <summary>
        /// Remove um papel (role) de um usuário.
        /// </summary>
        /// <param name="dto">Dados da remoção, contendo o ID do usuário, a role e o solicitante.</param>
        /// <returns>Verdadeiro se a operação for concluída com sucesso.</returns>
        public async Task<ServiceResultDto<bool>> RemoveFromRoleAsync(ManageUserRoleDto dto)
        {
            logger.LogInformation("Tentando remover papel {Role} do usuário {UserId} por {RequesterId}", dto.RoleName, dto.UserId, dto.RequesterId);

            var requester = await userManager.FindByIdAsync(dto.RequesterId.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator) || await userManager.IsLockedOutAsync(requester))
            {
                logger.LogWarning("Permissão negada ou conta inativa ao tentar remover papel.");
                return new ServiceResultDto<bool>(false, false, "Você não tem permissão ou sua conta está desativada.");
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

        /// <summary>
        /// Gerencia o status de bloqueio de um usuário (ativação/desativação).
        /// </summary>
        /// <param name="dto">Dados do status, contendo o ID do usuário, o novo estado e o solicitante.</param>
        /// <returns>Verdadeiro se o status foi alterado com sucesso.</returns>
        public async Task<ServiceResultDto<bool>> ManageUserStatusAsync(ManageUserStatusDto dto)
        {
            logger.LogInformation("Tentando alterar status de bloqueio do usuário {UserId} para {IsLocked} por {RequesterId}", dto.UserId, dto.IsLocked, dto.RequesterId);

            var requester = await userManager.FindByIdAsync(dto.RequesterId.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator) || await userManager.IsLockedOutAsync(requester))
            {
                logger.LogWarning("Permissão negada ou conta inativa ao tentar gerenciar status do usuário.");
                return new ServiceResultDto<bool>(false, false, "Você não tem permissão ou sua conta está desativada.");
            }

            var user = await userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
            {
                logger.LogWarning("Usuário {UserId} não encontrado para alteração de status.", dto.UserId);
                return new ServiceResultDto<bool>(false, false, "Usuário não encontrado.");
            }

            IdentityResult result;
            if (dto.IsLocked)
            {
                await userManager.SetLockoutEnabledAsync(user, true);
                result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            else
            {
                result = await userManager.SetLockoutEndDateAsync(user, null);
            }

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                logger.LogError("Erro ao alterar status do usuário {UserId}: {Errors}", dto.UserId, errors);
                return new ServiceResultDto<bool>(false, false, $"Erro: {errors}");
            }

            await userManager.UpdateSecurityStampAsync(user);

            var action = dto.IsLocked ? "desabilitado" : "habilitado";
            logger.LogInformation("Usuário {UserId} {Action} com sucesso.", dto.UserId, action);
            return new ServiceResultDto<bool>(true, true, $"Usuário {action} com sucesso.");
        }

        /// <summary>
        /// Altera a senha de um usuário.
        /// Suporta tanto o reset administrativo quanto a alteração por autoatendimento.
        /// </summary>
        /// <param name="dto">Dados da alteração, incluindo identificação, nova senha e opcionalmente a senha atual.</param>
        /// <returns>Verdadeiro se a senha foi alterada com sucesso.</returns>
        /// <remarks>
        /// Se o solicitante for um administrador, a senha atual é ignorada. 
        /// Se for o próprio usuário, a senha atual é obrigatória para validação de segurança.
        /// </remarks>
        public async Task<ServiceResultDto<bool>> ChangePasswordAsync(ChangePasswordDto dto)
        {
            logger.LogInformation("Solicitação de alteração de senha para {Email} solicitada por {RequesterId}", dto.Email, dto.RequesterId);

            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                var errors = string.Join(" ", validationResults.Select(r => r.ErrorMessage));
                return new ServiceResultDto<bool>(false, false, $"Dados inválidos: {errors}");
            }

            var requester = await userManager.FindByIdAsync(dto.RequesterId.ToString());
            if (requester == null || await userManager.IsLockedOutAsync(requester))
            {
                logger.LogWarning("Solicitante {RequesterId} não encontrado ou inativo.", dto.RequesterId);
                return new ServiceResultDto<bool>(false, false, "Acesso negado: Solicitante não encontrado ou conta desativada.");
            }

            var targetUser = await userManager.FindByEmailAsync(dto.Email);
            if (targetUser == null)
            {
                logger.LogWarning("Usuário com e-mail {Email} não encontrado para alteração de senha.", dto.Email);
                return new ServiceResultDto<bool>(false, false, "Usuário não encontrado.");
            }

            bool isAdmin = await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator);
            bool isSelf = requester.Id == targetUser.Id;

            if (!isAdmin && !isSelf)
            {
                logger.LogWarning("Usuário {RequesterId} tentou alterar a senha de {Email} sem permissão.", dto.RequesterId, dto.Email);
                return new ServiceResultDto<bool>(false, false, "Você não tem permissão para alterar esta senha.");
            }

            IdentityResult result;

            if (isAdmin && !isSelf)
            {
                logger.LogInformation("Administrador {RequesterId} resetando senha de {Email}.", dto.RequesterId, dto.Email);
                var token = await userManager.GeneratePasswordResetTokenAsync(targetUser);
                result = await userManager.ResetPasswordAsync(targetUser, token, dto.Password);
            }
            else
            {
                if (string.IsNullOrEmpty(dto.CurrentPassword))
                {
                    return new ServiceResultDto<bool>(false, false, "A senha atual é obrigatória para esta operação.");
                }

                result = await userManager.ChangePasswordAsync(targetUser, dto.CurrentPassword, dto.Password);
            }

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                logger.LogError("Erro ao alterar senha de {Email}: {Errors}", dto.Email, errors);
                return new ServiceResultDto<bool>(false, false, $"Erro ao alterar senha: {errors}");
            }

            await userManager.UpdateSecurityStampAsync(targetUser);
            
            logger.LogInformation("Senha de {Email} alterada com sucesso.", dto.Email);
            return new ServiceResultDto<bool>(true, true, "Senha alterada com sucesso.");
        }

        /// <summary>
        /// Obtém uma lista resumida de todos os usuários cadastrados.
        /// </summary>
        public async Task<ServiceResultDto<List<UserSummaryDto>>> GetUsersSummaryAsync(Guid requesterId)
        {
            var requester = await userManager.FindByIdAsync(requesterId.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator) || await userManager.IsLockedOutAsync(requester))
            {
                return new ServiceResultDto<List<UserSummaryDto>>(false, [], "Acesso negado.");
            }

            var users = await userManager.Users.ToListAsync();
            var summaries = new List<UserSummaryDto>();

            foreach (var user in users)
            {
                var roles = (await userManager.GetRolesAsync(user)).ToList();
                var isLockedOut = await userManager.IsLockedOutAsync(user);
                summaries.Add(new UserSummaryDto(user.Id, user.Email!, isLockedOut, roles));
            }

            return new ServiceResultDto<List<UserSummaryDto>>(true, summaries, "Usuários carregados com sucesso.");
        }

        /// <summary>
        /// Atualiza o e-mail e username de um usuário.
        /// </summary>
        public async Task<ServiceResultDto<bool>> UpdateUserEmailAsync(Guid requesterId, Guid userId, string newEmail)
        {
            var requester = await userManager.FindByIdAsync(requesterId.ToString());
            if (requester == null || !await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator) || await userManager.IsLockedOutAsync(requester))
            {
                return new ServiceResultDto<bool>(false, false, "Acesso negado.");
            }

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null) return new ServiceResultDto<bool>(false, false, "Usuário não encontrado.");

            user.Email = newEmail;
            user.UserName = newEmail;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                return new ServiceResultDto<bool>(false, false, $"Erro ao atualizar usuário: {errors}");
            }

            return new ServiceResultDto<bool>(true, true, "Usuário atualizado com sucesso.");
        }
    }
}
