using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Dtos.User
{
    /// <summary>
    /// Objeto de transferência de dados para gerenciamento do status de bloqueio de um usuário.
    /// </summary>
    /// <param name="RequesterId">ID do administrador que realiza a operação.</param>
    /// <param name="UserId">ID do usuário alvo que terá seu status alterado.</param>
    /// <param name="IsLocked">Define se o usuário deve ser bloqueado (true) ou desbloqueado (false).</param>
    public record ManageUserStatusDto(
        [Required(ErrorMessage = "O ID do administrador solicitante é obrigatório.")]
        Guid RequesterId,

        [Required(ErrorMessage = "O ID do usuário alvo é obrigatório.")]
        Guid UserId,

        [Required(ErrorMessage = "O novo estado de bloqueio deve ser especificado.")]
        bool IsLocked
    );
}
