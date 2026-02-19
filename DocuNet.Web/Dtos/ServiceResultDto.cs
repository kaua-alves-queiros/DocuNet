namespace DocuNet.Web.Dtos
{
    /// <summary>
    /// Estrutura padronizada para o retorno de operações na camada de serviço.
    /// </summary>
    /// <typeparam name="T">O tipo do objeto de dados retornado em caso de sucesso.</typeparam>
    /// <param name="Success">Indica se a operação foi concluída com êxito.</param>
    /// <param name="Data">Os dados retornados pela operação. Pode ser nulo se a operação falhar ou não tiver retorno.</param>
    /// <param name="Message">Mensagem descritiva sobre o resultado (ex: mensagens de erro ou confirmação).</param>
    public record ServiceResultDto<T>(
        bool Success,
        T? Data = default,
        string Message = ""
    );
}
