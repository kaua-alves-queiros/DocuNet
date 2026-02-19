using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Dtos.User
{
    /// <summary>
    /// DTO para gerenciar a atribuição ou remoção de papéis (roles) de um usuário.
    /// </summary>
    /// <param name="RequesterId">ID do usuário que está realizando a operação.</param>
    /// <param name="UserId">ID do usuário que receberá ou perderá o papel.</param>
    /// <param name="RoleName">Nome do papel a ser gerenciado.</param>
    public record ManageUserRoleDto(
        [Required]
        Guid RequesterId,

        [Required]
        Guid UserId,

        [Required]
        string RoleName
    );
}
