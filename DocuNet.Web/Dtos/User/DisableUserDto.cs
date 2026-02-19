using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Dtos.User
{
    /// <summary>
    /// Objeto de transferência de dados para a desativação de usuários.
    /// </summary>
    /// <param name="RequesterId">ID do administrador que executa a operação. Deve possuir privilégios de Admin.</param>
    /// <param name="UserId">ID do usuário alvo que será desabilitado.</param>
    public record DisableUserDto(
        [Required(ErrorMessage = "O ID do administrador solicitante é obrigatório.")]
        [RegularExpression(@"^(?!00000000-0000-0000-0000-000000000000).*", ErrorMessage = "O RequesterId inválido.")]
        Guid RequesterId,

        [Required(ErrorMessage = "O ID do usuário a ser desabilitado é obrigatório.")]
        [RegularExpression(@"^(?!00000000-0000-0000-0000-000000000000).*", ErrorMessage = "O UserId não pode ser um GUID vazio.")]
        Guid UserId
    );
}
