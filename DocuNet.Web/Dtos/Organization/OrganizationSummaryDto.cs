namespace DocuNet.Web.Dtos.Organization;

/// <summary>
/// DTO para listagem resumida de organizações.
/// </summary>
public record OrganizationSummaryDto(
    Guid Id,
    string Name,
    bool IsActive,
    int MemberCount
);
