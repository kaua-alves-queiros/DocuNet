using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Constants;
using DocuNet.Web.Data;
using DocuNet.Web.Dtos;
using DocuNet.Web.Dtos.Device;
using DocuNet.Web.Enumerators;
using DocuNet.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocuNet.Web.Services
{
    /// <summary>
    /// Serviço responsável pela gestão de devices no sistema.
    /// </summary>
    public class DeviceService(
        ApplicationDatabaseContext context,
        ILogger<DeviceService> logger,
        UserManager<User> userManager)
    {
        /// <summary>
        /// Cria um novo device associado a uma organização.
        /// Membros da organização podem adicionar devices; 
        /// Admins podem adicionar a qualquer organização.
        /// </summary>
        public async Task<ServiceResultDto<Guid>> CreateDeviceAsync(CreateDeviceDto dto)
        {
            logger.LogInformation("Iniciando criação de device '{Name}' para Organização {OrgId} solicitada por {RequesterId}", dto.Name, dto.OrganizationId, dto.RequesterId);

            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                var errors = string.Join(" ", validationResults.Select(r => r.ErrorMessage));
                logger.LogWarning("Validação falhou para criação de device: {Errors}", errors);
                return new ServiceResultDto<Guid>(false, Guid.Empty, $"Dados inválidos: {errors}");
            }

            var requester = await userManager.FindByIdAsync(dto.RequesterId.ToString());
            if (requester == null || await userManager.IsLockedOutAsync(requester))
            {
                logger.LogWarning("Solicitante {RequesterId} não encontrado ou bloqueado.", dto.RequesterId);
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Solicitante inválido ou inativo.");
            }

            bool hasPermission = false;

            if (await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator))
            {
                hasPermission = true;
            }
            else
            {
                // Verifica se o usuário pertence à organização alvo
                var organization = await context.Organizations
                    .Include(o => o.Users)
                    .FirstOrDefaultAsync(o => o.Id == dto.OrganizationId);

                if (organization != null && organization.Users.Any(u => u.Id == dto.RequesterId))
                {
                    hasPermission = true;
                }
            }

            if (!hasPermission)
            {
                logger.LogWarning("Acesso negado para {RequesterId} criar device na organização {OrgId}.", dto.RequesterId, dto.OrganizationId);
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Acesso negado: Você não tem permissão para adicionar dispositivos nesta organização.");
            }

            // Verifica se a organização existe e está ativa
            var targetOrg = await context.Organizations.FindAsync(dto.OrganizationId);
            if (targetOrg == null)
            {
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Organização não encontrada.");
            }
            if (!targetOrg.IsActive)
            {
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Não é possível adicionar dispositivos a uma organização inativa.");
            }

            // Verifica se o nome já existe na organização
            if (await context.Devices.AnyAsync(d => d.OrganizationId == dto.OrganizationId && d.Name.ToLower() == dto.Name.ToLower()))
            {
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Já existe um dispositivo com este nome nesta organização.");
            }

            var device = new Device
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                IpAddress = dto.IpAddress,
                Type = dto.Type,
                OrganizationId = dto.OrganizationId
            };

            try
            {
                context.Devices.Add(device);
                await context.SaveChangesAsync();
                logger.LogInformation("Device {Id} criado com sucesso.", device.Id);
                return new ServiceResultDto<Guid>(true, device.Id, "Dispositivo criado com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao salvar device no banco de dados.");
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Erro interno ao criar dispositivo.");
            }
        }

        /// <summary>
        /// Lista devices visíveis ao solicitante.
        /// Admins veem todos; 
        /// Usuários veem apenas devices de suas organizações.
        /// </summary>
        public async Task<ServiceResultDto<List<DeviceSummaryDto>>> GetDevicesAsync(Guid requesterId)
        {
            var requester = await userManager.FindByIdAsync(requesterId.ToString());
            if (requester == null || await userManager.IsLockedOutAsync(requester))
            {
                return new ServiceResultDto<List<DeviceSummaryDto>>(false, [], "Acesso negado.");
            }

            var isAdmin = await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator);

            List<DeviceSummaryDto> devices;

            if (isAdmin)
            {
                devices = await context.Devices
                    .Include(d => d.Organization)
                    .Select(d => new DeviceSummaryDto(d.Id, d.Name, d.IpAddress ?? "", d.Type, d.Organization.Name, d.OrganizationId))
                    .ToListAsync();
            }
            else
            {
                // Busca apenas devices das organizações onde o usuário é membro
                var userOrgIds = await context.Organizations
                    .Where(o => o.Users.Any(u => u.Id == requesterId))
                    .Select(o => o.Id)
                    .ToListAsync();

                devices = await context.Devices
                    .Include(d => d.Organization)
                    .Where(d => userOrgIds.Contains(d.OrganizationId))
                    .Select(d => new DeviceSummaryDto(d.Id, d.Name, d.IpAddress ?? "", d.Type, d.Organization.Name, d.OrganizationId))
                    .ToListAsync();
            }

            return new ServiceResultDto<List<DeviceSummaryDto>>(true, devices, "Dispositivos listados com sucesso.");
        }

        /// <summary>
        /// Remove um device.
        /// Segue a mesma lógica de permissão de adição.
        /// </summary>
        public async Task<ServiceResultDto<bool>> DeleteDeviceAsync(DeleteDeviceDto dto)
        {
            logger.LogInformation("Solicitação de remoção de device {DeviceId} por {RequesterId}", dto.DeviceId, dto.RequesterId);

            var requester = await userManager.FindByIdAsync(dto.RequesterId.ToString());
            if (requester == null || await userManager.IsLockedOutAsync(requester))
            {
                return new ServiceResultDto<bool>(false, false, "Solicitante inválido.");
            }

            var device = await context.Devices.FindAsync(dto.DeviceId);
            if (device == null)
            {
                return new ServiceResultDto<bool>(false, false, "Dispositivo não encontrado.");
            }

            bool hasPermission = false;
            if (await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator))
            {
                hasPermission = true;
            }
            else
            {
                // Verifica se o usuário pertence à mesma organização do device
                var org = await context.Organizations
                    .Include(o => o.Users)
                    .FirstOrDefaultAsync(o => o.Id == device.OrganizationId);

                if (org != null && org.Users.Any(u => u.Id == dto.RequesterId))
                {
                    hasPermission = true;
                }
            }

            if (!hasPermission)
            {
                logger.LogWarning("Tentativa de remoção de device negada para {RequesterId}", dto.RequesterId);
                return new ServiceResultDto<bool>(false, false, "Acesso negado: Você não tem permissão para remover este dispositivo.");
            }

            try
            {
                context.Devices.Remove(device);
                await context.SaveChangesAsync();
                logger.LogInformation("Device {DeviceId} removido com sucesso.", dto.DeviceId);
                return new ServiceResultDto<bool>(true, true, "Dispositivo removido com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao remover device {DeviceId}", dto.DeviceId);
                return new ServiceResultDto<bool>(false, false, "Erro interno ao remover dispositivo.");
            }
        }

        /// <summary>
        /// Atualiza os dados de um dispositivo (Nome, IP, Tipo).
        /// Segue a mesma lógica de permissão de adição.
        /// </summary>
        public async Task<ServiceResultDto<bool>> UpdateDeviceAsync(UpdateDeviceDto dto)
        {
            logger.LogInformation("Solicitação de atualização de device {DeviceId} por {RequesterId}", dto.DeviceId, dto.RequesterId);

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
                return new ServiceResultDto<bool>(false, false, "Solicitante inválido ou inativo.");
            }

            var device = await context.Devices.FindAsync(dto.DeviceId);
            if (device == null)
            {
                return new ServiceResultDto<bool>(false, false, "Dispositivo não encontrado.");
            }

            bool hasPermission = false;
            // Admin tem permissão total
            if (await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator))
            {
                hasPermission = true;
            }
            else
            {
                // Verifica se o usuário pertence à mesma organização do device
                var org = await context.Organizations
                    .Include(o => o.Users)
                    .FirstOrDefaultAsync(o => o.Id == device.OrganizationId);

                if (org != null && org.Users.Any(u => u.Id == dto.RequesterId))
                {
                    hasPermission = true;
                }
            }

            if (!hasPermission)
            {
                logger.LogWarning("Tentativa de atualização de device negada para {RequesterId}", dto.RequesterId);
                return new ServiceResultDto<bool>(false, false, "Acesso negado: Você não tem permissão para atualizar este dispositivo.");
            }

            // Verifica unicidade do nome se ele for alterado
            if (dto.Name != null && dto.Name.ToLower() != device.Name.ToLower())
            {
                if (await context.Devices.AnyAsync(d => d.OrganizationId == device.OrganizationId && d.Name.ToLower() == dto.Name.ToLower() && d.Id != dto.DeviceId))
                {
                    return new ServiceResultDto<bool>(false, false, "Já existe outro dispositivo com este nome nesta organização.");
                }
            }

            if (dto.Name != null) device.Name = dto.Name;
            if (dto.IpAddress != null) device.IpAddress = dto.IpAddress;
            if (dto.Type.HasValue) device.Type = dto.Type.Value;

            try
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Device {DeviceId} atualizado com sucesso.", dto.DeviceId);
                return new ServiceResultDto<bool>(true, true, "Dispositivo atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao atualizar device {DeviceId}", dto.DeviceId);
                return new ServiceResultDto<bool>(false, false, "Erro interno ao atualizar dispositivo.");
            }
        }
    }
}
