namespace DocuNet.Web.Models
{
    public class Organization
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<User> Users { get; set; } = new List<User>();
        public bool IsActive { get; set; } = true;
    }
}
