using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Dtos.Organization
{
    /// <summary>
    /// DTO para adicionar ou remover um usuário de uma organização.
    /// Pode ser executado por um administrador ou por um usuário que já pertença à organização.
    /// </summary>
    /// <param name="RequesterId">ID do usuário solicitante (Admin ou membro).</param>
    /// <param name="OrganizationId">ID da organização alvo.</param>
    /// <param name="UserId">ID do usuário a ser adicionado ou removido.</param>
    public record ManageUserInOrganizationDto(
        [Required(ErrorMessage = "O ID do solicitante é obrigatório.")]
        Guid RequesterId, 
        
        [Required(ErrorMessage = "O ID da organização é obrigatório.")]
        Guid OrganizationId, 
        
        [Required(ErrorMessage = "O ID do usuário alvo é obrigatório.")]
        Guid UserId
    );
}
