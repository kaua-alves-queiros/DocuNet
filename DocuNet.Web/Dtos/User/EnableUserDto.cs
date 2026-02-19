using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Dtos.User
{
    /// <summary>
    /// Objeto de transferência de dados para a operação de ativação (reabilitação) de um usuário.
    /// </summary>
    /// <param name="RequesterId">ID do administrador que executa a ativação. Deve possuir privilégios de Admin.</param>
    /// <param name="UserId">ID do usuário alvo que terá o acesso restaurado.</param>
    public record EnableUserDto(
        [Required(ErrorMessage = "O ID do administrador solicitante é obrigatório.")]
        [RegularExpression(@"^(?!00000000-0000-0000-0000-000000000000).*", ErrorMessage = "O RequesterId deve ser um GUID válido.")]
        Guid RequesterId,

        [Required(ErrorMessage = "O ID do usuário a ser habilitado é obrigatório.")]
        [RegularExpression(@"^(?!00000000-0000-0000-0000-000000000000).*", ErrorMessage = "O UserId deve ser um GUID válido.")]
        Guid UserId
    );
}
