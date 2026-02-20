using Microsoft.AspNetCore.Identity;

namespace DocuNet.Web.Models
{
    /// <summary>
    /// Representa um usuário do sistema, estendendo a identidade do ASP.NET Core.
    /// </summary>
    public class User : IdentityUser<Guid>
    {
        /// <summary>
        /// Inicializa uma nova instância de usuário com um ID único.
        /// </summary>
        public User()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Organizações às quais este usuário pertence ou tem acesso.
        /// </summary>
        public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    }
}
