using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Dtos.User
{
    /// <summary>
    /// Objeto de transferência de dados para a criação de um novo usuário.
    /// </summary>
    /// <param name="CreatedBy">O identificador (GUID) do usuário ou administrador que está realizando a operação.</param>
    /// <param name="Email">O endereço de e-mail que será utilizado como login e identidade única do novo usuário.</param>
    /// <param name="Password">A senha de acesso, que deve seguir as políticas de complexidade do Identity.</param>
    /// <param name="ConfirmPassword">A confirmação da senha. Deve ser idêntica ao campo <paramref name="Password"/>.</param>
    public record CreateUserDto(
        Guid CreatedBy,

        [Required]
        [EmailAddress]
        string Email,

        [Required]
        string Password,

        [Required]
        [property: Compare("Password", ErrorMessage = "As senhas não conferem.")]
        string ConfirmPassword
    );
}
