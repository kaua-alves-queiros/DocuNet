using Microsoft.AspNetCore.Identity;

namespace DocuNet.Web.Models
{
    public class User : IdentityUser<Guid>
    {
        public User()
        {
            Id = Guid.NewGuid();
        }
    }
}
