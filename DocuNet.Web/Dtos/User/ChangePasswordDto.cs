using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DocuNet.Web.Dtos.User
{
    /// <summary>
    /// DTO para solicitação de alteração de senha.
    /// </summary>
    /// <remarks>
    /// <b>Regra de Negócio:</b> 
    /// Se o <paramref name="RequesterId"/> for um Administrador, o campo <paramref name="CurrentPassword"/> pode ser nulo.
    /// Se o <paramref name="RequesterId"/> for o próprio usuário, <paramref name="CurrentPassword"/> é obrigatório para validação de segurança.
    /// </remarks>
    /// <param name="RequesterId">ID do usuário que está solicitando a alteração (Admin ou o próprio usuário).</param>
    /// <param name="Email">E-mail da conta que terá a senha alterada.</param>
    /// <param name="CurrentPassword">Senha atual do usuário. Opcional apenas se a requisição for feita por um Administrador.</param>
    /// <param name="Password">A nova senha desejada.</param>
    /// <param name="ConfirmPassword">Confirmação da nova senha. Deve ser idêntica a <paramref name="Password"/>.</param>
    public record ChangePasswordDto(
        [Required]
        Guid RequesterId,

        [Required]
        [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
        string Email,

        [AllowNull]
        string? CurrentPassword,

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
        string Password,

        [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
        [property: Compare("Password", ErrorMessage = "As senhas não conferem.")]
        string ConfirmPassword
    );
}
