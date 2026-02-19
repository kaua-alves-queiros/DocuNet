using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Dtos.Organization
{
    /// <summary>
    /// DTO para renomear uma organização existente.
    /// Operação restrita a administradores do sistema.
    /// </summary>
    /// <param name="RequesterId">ID do administrador que solicita a alteração.</param>
    /// <param name="OrganizationId">ID da organização a ser renomeada.</param>
    /// <param name="NewName">Novo nome para a organização.</param>
    public record RenameOrganizationDto(
        [Required(ErrorMessage = "O ID do solicitante é obrigatório.")]
        Guid RequesterId,

        [Required(ErrorMessage = "O ID da organização é obrigatório.")]
        Guid OrganizationId,

        [Required(ErrorMessage = "O novo nome da organização é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        string NewName
    );
}
