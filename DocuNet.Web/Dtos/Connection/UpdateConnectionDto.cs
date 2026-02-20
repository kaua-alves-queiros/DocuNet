using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Enumerators;

namespace DocuNet.Web.Dtos.Connection;

/// <summary>
/// DTO para atualização de uma conexão existente.
/// Todos os campos (exceto IDs obrigatórios) são opcionais para permitir atualizações parciais.
/// </summary>
public record UpdateConnectionDto(
    [Required(ErrorMessage = "O ID do solicitante é obrigatório.")]
    Guid RequesterId,

    [Required(ErrorMessage = "O ID da conexão é obrigatório.")]
    Guid ConnectionId,

    Guid? SourceDeviceId = null,
    string? SourceInterface = null,
    Guid? DestinationDeviceId = null,
    string? DestinationInterface = null,
    EConnectionTypes? Type = null,
    string? Speed = null
);
