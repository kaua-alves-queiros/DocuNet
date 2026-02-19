using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Dtos.Organization
{
    /// <summary>
    /// DTO para criação de uma nova organização no sistema.
    /// Operação restrita a administradores do sistema.
    /// </summary>
    /// <param name="CreatedBy">ID do administrador que está criando a organização.</param>
    /// <param name="Name">Nome da organização a ser criada.</param>
    public record CreateOrganizationDto(
        [Required(ErrorMessage = "O ID do criador é obrigatório.")]
        Guid CreatedBy, 
        
        [Required(ErrorMessage = "O nome da organização é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        string Name
    );
}
