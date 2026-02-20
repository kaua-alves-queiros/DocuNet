using DocuNet.Web.Enumerators;

namespace DocuNet.Web.Dtos.Connection;

/// <summary>
/// DTO resumido para listagem de conexões.
/// </summary>
public record ConnectionSummaryDto(
    Guid Id,
    Guid SourceDeviceId,
    string SourceDeviceName,
    string? SourceInterface,
    Guid DestinationDeviceId,
    string DestinationDeviceName,
    string? DestinationInterface,
    EConnectionTypes Type,
    string? Speed,
    Guid OrganizationId
);
