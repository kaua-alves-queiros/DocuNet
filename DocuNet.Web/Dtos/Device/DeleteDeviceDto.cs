using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Dtos.Device;

/// <summary>
/// DTO para exclusão de um dispositivo.
/// </summary>
/// <param name="RequesterId">ID do usuário solicitante.</param>
/// <param name="DeviceId">ID do dispositivo a ser excluído.</param>
public record DeleteDeviceDto(
    [Required(ErrorMessage = "O ID do solicitante é obrigatório.")]
    Guid RequesterId,
    
    [Required(ErrorMessage = "O ID do dispositivo é obrigatório.")]
    Guid DeviceId
);
