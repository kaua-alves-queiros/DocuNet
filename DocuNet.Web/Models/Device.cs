using DocuNet.Web.Enumerators;

namespace DocuNet.Web.Models
{
    public class Device
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public EDeviceTypes Type { get; set; }
        public string? IpAddress { get; set; } = null!;
        public Guid OrganizationId { get; set; } = default!;
        public Organization Organization { get; set; } = default!;
    }
}
