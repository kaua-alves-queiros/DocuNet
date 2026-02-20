using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Enumerators;

namespace DocuNet.Web.Dtos.Device;

/// <summary>
/// DTO para criação de um novo dispositivo.
/// </summary>
/// <param name="RequesterId">ID do usuário solicitante.</param>
/// <param name="Name">Nome do dispositivo.</param>
/// <param name="IpAddress">Endereço IP do dispositivo.</param>
/// <param name="Type">Tipo do dispositivo (Enum).</param>
/// <param name="OrganizationId">ID da organização à qual o dispositivo pertencerá.</param>
public record CreateDeviceDto(
    [Required(ErrorMessage = "O ID do solicitante é obrigatório.")]
    Guid RequesterId,
    
    [Required(ErrorMessage = "O nome do dispositivo é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    string Name,
    
    [StringLength(50, ErrorMessage = "O endereço IP não pode exceder 50 caracteres.")]
    string? IpAddress,
    
    [Required(ErrorMessage = "O tipo do dispositivo é obrigatório.")]
    EDeviceTypes Type,
    
    [Required(ErrorMessage = "O ID da organização é obrigatório.")]
    Guid OrganizationId
);
