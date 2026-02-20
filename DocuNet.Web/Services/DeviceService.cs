using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Constants;
using DocuNet.Web.Data;
using DocuNet.Web.Dtos;
using DocuNet.Web.Dtos.Device;
using DocuNet.Web.Dtos.Connection;
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

        #region Connections Management

        /// <summary>
        /// Cria uma nova conexão entre dois dispositivos.
        /// Regras de Acesso: Administradores de Sistema têm acesso total; Membros podem criar conexões apenas se ambos os dispositivos pertencerem a uma organização na qual são membros.
        /// </summary>
        public async Task<ServiceResultDto<Guid>> CreateConnectionAsync(CreateConnectionDto dto)
        {
            logger.LogInformation("Solicitação de criação de conexão entre {Source} e {Dest} por {RequesterId}", dto.SourceDeviceId, dto.DestinationDeviceId, dto.RequesterId);

            if (dto.SourceDeviceId == dto.DestinationDeviceId)
            {
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Não é possível conectar um dispositivo a ele mesmo.");
            }

            var requester = await userManager.FindByIdAsync(dto.RequesterId.ToString());
            if (requester == null || await userManager.IsLockedOutAsync(requester))
            {
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Solicitante inválido ou inativo.");
            }

            var source = await context.Devices.FindAsync(dto.SourceDeviceId);
            var dest = await context.Devices.FindAsync(dto.DestinationDeviceId);

            if (source == null || dest == null)
            {
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Um ou ambos os dispositivos não foram encontrados.");
            }

            // Garante que ambos os dispositivos pertencem à mesma organização informada
            if (source.OrganizationId != dto.OrganizationId || dest.OrganizationId != dto.OrganizationId)
            {
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Os dispositivos devem pertencer à mesma organização.");
            }

            bool hasPermission = false;
            if (await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator))
            {
                hasPermission = true;
            }
            else
            {
                // Membros só podem criar se pertencerem à organização alvo
                var isMember = await context.Organizations
                    .AnyAsync(o => o.Id == dto.OrganizationId && o.Users.Any(u => u.Id == dto.RequesterId));

                if (isMember)
                {
                    hasPermission = true;
                }
            }

            if (!hasPermission)
            {
                logger.LogWarning("Tentativa negada de criar conexão: {RequesterId} não tem permissão para a organização {OrgId}", dto.RequesterId, dto.OrganizationId);
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Acesso negado: Você não tem permissão para gerenciar conexões nesta organização.");
            }

            var connection = new Connection
            {
                Id = Guid.NewGuid(),
                SourceDeviceId = dto.SourceDeviceId,
                SourceInterface = dto.SourceInterface,
                DestinationDeviceId = dto.DestinationDeviceId,
                DestinationInterface = dto.DestinationInterface,
                Type = dto.Type,
                Speed = dto.Speed,
                OrganizationId = dto.OrganizationId
            };

            try
            {
                context.Connections.Add(connection);
                await context.SaveChangesAsync();
                logger.LogInformation("Conexão {Id} criada com sucesso por {RequesterId}", connection.Id, dto.RequesterId);
                return new ServiceResultDto<Guid>(true, connection.Id, "Conexão criada com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao criar conexão.");
                return new ServiceResultDto<Guid>(false, Guid.Empty, "Erro interno ao criar conexão.");
            }
        }

        /// <summary>
        /// Lista conexões visíveis ao solicitante.
        /// Regras de Acesso: Administradores de Sistema visualizam todas as conexões de todas as organizações; Membros visualizam apenas conexões das organizações em que estão inseridos.
        /// </summary>
        public async Task<ServiceResultDto<List<ConnectionSummaryDto>>> GetConnectionsAsync(Guid requesterId)
        {
            var requester = await userManager.FindByIdAsync(requesterId.ToString());
            if (requester == null || await userManager.IsLockedOutAsync(requester))
            {
                return new ServiceResultDto<List<ConnectionSummaryDto>>(false, [], "Acesso negado.");
            }

            var isAdmin = await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator);
            
            IQueryable<Connection> query = context.Connections
                .Include(c => c.SourceDevice)
                .Include(c => c.DestinationDevice);

            if (!isAdmin)
            {
                // Filtra apenas conexões de organizações onde o usuário é membro
                var userOrgIds = await context.Organizations
                    .Where(o => o.Users.Any(u => u.Id == requesterId))
                    .Select(o => o.Id)
                    .ToListAsync();

                query = query.Where(c => userOrgIds.Contains(c.OrganizationId));
            }

            var connections = await query.Select(c => new ConnectionSummaryDto(
                c.Id,
                c.SourceDeviceId,
                c.SourceDevice.Name,
                c.SourceInterface,
                c.DestinationDeviceId,
                c.DestinationDevice.Name,
                c.DestinationInterface,
                c.Type,
                c.Speed,
                c.OrganizationId
            )).ToListAsync();

            return new ServiceResultDto<List<ConnectionSummaryDto>>(true, connections, "Conexões listadas com sucesso.");
        }

        /// <summary>
        /// Remove uma conexão existente.
        /// Regras de Acesso: Administradores de Sistema podem remover qualquer conexão; Membros podem remover apenas se a conexão pertencer a uma organização na qual são membros.
        /// </summary>
        public async Task<ServiceResultDto<bool>> DeleteConnectionAsync(Guid requesterId, Guid connectionId)
        {
            logger.LogInformation("Solicitação de remoção de conexão {ConnId} por {RequesterId}", connectionId, requesterId);

            var requester = await userManager.FindByIdAsync(requesterId.ToString());
            if (requester == null || await userManager.IsLockedOutAsync(requester))
            {
                return new ServiceResultDto<bool>(false, false, "Solicitante inválido.");
            }

            var connection = await context.Connections.FindAsync(connectionId);
            if (connection == null)
            {
                return new ServiceResultDto<bool>(false, false, "Conexão não encontrada.");
            }

            bool hasPermission = false;
            if (await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator))
            {
                hasPermission = true;
            }
            else
            {
                // Verifica se o usuário é membro da organização da conexão
                var isMember = await context.Organizations
                    .AnyAsync(o => o.Id == connection.OrganizationId && o.Users.Any(u => u.Id == requesterId));

                if (isMember)
                {
                    hasPermission = true;
                }
            }

            if (!hasPermission)
            {
                logger.LogWarning("Tentativa de remoção de conexão negada para {RequesterId}", requesterId);
                return new ServiceResultDto<bool>(false, false, "Acesso negado: Você não tem permissão para remover esta conexão.");
            }

            try
            {
                context.Connections.Remove(connection);
                await context.SaveChangesAsync();
                logger.LogInformation("Conexão {ConnId} removida com sucesso por {RequesterId}", connectionId, requesterId);
                return new ServiceResultDto<bool>(true, true, "Conexão removida com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao remover conexão {ConnId}", connectionId);
                return new ServiceResultDto<bool>(false, false, "Erro interno ao remover conexão.");
            }
        }

        /// <summary>
        /// Atualiza uma conexão existente.
        /// Regras de Acesso: Administradores de Sistema podem atualizar qualquer conexão; Membros podem atualizar se a conexão pertencer a uma organização na qual são membros.
        /// </summary>
        public async Task<ServiceResultDto<bool>> UpdateConnectionAsync(UpdateConnectionDto dto)
        {
            logger.LogInformation("Solicitação de atualização da conexão {ConnId} por {RequesterId}", dto.ConnectionId, dto.RequesterId);

            var requester = await userManager.FindByIdAsync(dto.RequesterId.ToString());
            if (requester == null || await userManager.IsLockedOutAsync(requester))
            {
                return new ServiceResultDto<bool>(false, false, "Solicitante inválido ou inativo.");
            }

            var connection = await context.Connections.FindAsync(dto.ConnectionId);
            if (connection == null)
            {
                return new ServiceResultDto<bool>(false, false, "Conexão não encontrada.");
            }

            bool hasPermission = false;
            if (await userManager.IsInRoleAsync(requester, SystemRoles.SystemAdministrator))
            {
                hasPermission = true;
            }
            else
            {
                var isMember = await context.Organizations
                    .AnyAsync(o => o.Id == connection.OrganizationId && o.Users.Any(u => u.Id == dto.RequesterId));

                if (isMember)
                {
                    hasPermission = true;
                }
            }

            if (!hasPermission)
            {
                return new ServiceResultDto<bool>(false, false, "Acesso negado: Você não tem permissão para editar esta conexão.");
            }

            // Se mudar os dispositivos, valida se eles são da mesma organização e se não é auto-conexão
            Guid finalSource = dto.SourceDeviceId ?? connection.SourceDeviceId;
            Guid finalDest = dto.DestinationDeviceId ?? connection.DestinationDeviceId;

            if (finalSource == finalDest)
            {
                return new ServiceResultDto<bool>(false, false, "Não é possível conectar um dispositivo a ele mesmo.");
            }

            if (dto.SourceDeviceId.HasValue || dto.DestinationDeviceId.HasValue)
            {
                var source = await context.Devices.FindAsync(finalSource);
                var dest = await context.Devices.FindAsync(finalDest);

                if (source == null || dest == null)
                {
                    return new ServiceResultDto<bool>(false, false, "Dispositivo não encontrado.");
                }

                if (source.OrganizationId != connection.OrganizationId || dest.OrganizationId != connection.OrganizationId)
                {
                    return new ServiceResultDto<bool>(false, false, "Os dispositivos devem pertencer à mesma organização da conexão original.");
                }
            }

            // Atualiza campos
            if (dto.SourceDeviceId.HasValue) connection.SourceDeviceId = dto.SourceDeviceId.Value;
            if (dto.SourceInterface != null) connection.SourceInterface = dto.SourceInterface;
            if (dto.DestinationDeviceId.HasValue) connection.DestinationDeviceId = dto.DestinationDeviceId.Value;
            if (dto.DestinationInterface != null) connection.DestinationInterface = dto.DestinationInterface;
            if (dto.Type.HasValue) connection.Type = dto.Type.Value;
            if (dto.Speed != null) connection.Speed = dto.Speed;

            try
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Conexão {ConnId} atualizada com sucesso por {RequesterId}", dto.ConnectionId, dto.RequesterId);
                return new ServiceResultDto<bool>(true, true, "Conexão atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao atualizar conexão {ConnId}", dto.ConnectionId);
                return new ServiceResultDto<bool>(false, false, "Erro interno ao atualizar conexão.");
            }
        }

        #endregion
    }
}
