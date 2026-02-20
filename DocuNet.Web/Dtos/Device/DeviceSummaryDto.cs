using DocuNet.Web.Enumerators;

namespace DocuNet.Web.Dtos.Device;

/// <summary>
/// DTO para listagem resumida de dispositivos.
/// </summary>
/// <param name="Id">ID do dispositivo.</param>
/// <param name="Name">Nome do dispositivo.</param>
/// <param name="IpAddress">Endereço IP.</param>
/// <param name="Type">Tipo do dispositivo.</param>
/// <param name="OrganizationName">Nome da organização proprietária.</param>
/// <param name="OrganizationId">ID da organização proprietária.</param>
public record DeviceSummaryDto(
    Guid Id,
    string Name,
    string IpAddress,
    EDeviceTypes Type,
    string OrganizationName,
    Guid OrganizationId
);
