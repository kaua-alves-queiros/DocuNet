using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Enumerators;

namespace DocuNet.Web.Dtos.Connection;

/// <summary>
/// DTO para criação de uma nova conexão entre dispositivos.
/// </summary>
public record CreateConnectionDto(
    [Required(ErrorMessage = "O ID do solicitante é obrigatório.")]
    Guid RequesterId,

    [Required(ErrorMessage = "O dispositivo de origem é obrigatório.")]
    Guid SourceDeviceId,

    [Required(ErrorMessage = "O dispositivo de destino é obrigatório.")]
    Guid DestinationDeviceId,

    [Required(ErrorMessage = "O tipo de conexão é obrigatório.")]
    EConnectionTypes Type,

    [StringLength(50, ErrorMessage = "A interface de origem não pode exceder 50 caracteres.")]
    string? SourceInterface,

    [StringLength(50, ErrorMessage = "A interface de destino não pode exceder 50 caracteres.")]
    string? DestinationInterface,

    [StringLength(50, ErrorMessage = "A velocidade não pode exceder 50 caracteres.")]
    string? Speed,

    [Required(ErrorMessage = "A organização é obrigatória.")]
    Guid OrganizationId
);
