using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Enumerators;

namespace DocuNet.Web.Dtos.Device;

/// <summary>
/// DTO para atualização de um dispositivo.
/// </summary>
/// <param name="RequesterId">ID do usuário solicitante.</param>
/// <param name="DeviceId">ID do dispositivo a ser atualizado.</param>
/// <param name="Name">Novo nome (opcional).</param>
/// <param name="IpAddress">Novo endereço IP (opcional).</param>
/// <param name="Type">Novo tipo (opcional).</param>
public record UpdateDeviceDto(
    [Required(ErrorMessage = "O ID do solicitante é obrigatório.")]
    Guid RequesterId,
    
    [Required(ErrorMessage = "O ID do dispositivo é obrigatório.")]
    Guid DeviceId,
    
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    string? Name = null,
    
    [StringLength(50, ErrorMessage = "O endereço IP não pode exceder 50 caracteres.")]
    string? IpAddress = null,
    
    EDeviceTypes? Type = null
);
