using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Dtos.Organization
{
    /// <summary>
    /// DTO para ativar ou desativar uma organização.
    /// Operação restrita a administradores do sistema.
    /// </summary>
    /// <param name="RequesterId">ID do administrador solicitante.</param>
    /// <param name="OrganizationId">ID da organização alvo.</param>
    /// <param name="IsEnabled">Define se a organização será habilitada (true) ou desabilitada (false).</param>
    public record ManageOrganizationStatusDto(
        [Required(ErrorMessage = "O ID do solicitante é obrigatório.")]
        Guid RequesterId, 
        
        [Required(ErrorMessage = "O ID da organização é obrigatório.")]
        Guid OrganizationId, 
        
        [Required(ErrorMessage = "O estado (habilitado/desabilitado) deve ser especificado.")]
        bool IsEnabled
    );
}
