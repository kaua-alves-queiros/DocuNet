namespace DocuNet.Web.Dtos.User
{
    public record UserSummaryDto(
        Guid Id,
        string Email,
        bool IsLockedOut,
        List<string> Roles
    );
}
