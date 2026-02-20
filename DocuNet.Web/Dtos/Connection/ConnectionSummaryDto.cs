using DocuNet.Web.Enumerators;

namespace DocuNet.Web.Dtos.Connection;

/// <summary>
/// DTO resumido para listagem de conexões.
/// </summary>
public record ConnectionSummaryDto(
    Guid Id,
    Guid SourceDeviceId,
    string SourceDeviceName,
    EDeviceTypes SourceDeviceType,
    string? SourceInterface,
    Guid DestinationDeviceId,
    string DestinationDeviceName,
    EDeviceTypes DestinationDeviceType,
    string? DestinationInterface,
    EConnectionTypes Type,
    string? Speed,
    Guid OrganizationId
);
