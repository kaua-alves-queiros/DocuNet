using DocuNet.Web.Enumerators;

namespace DocuNet.Web.Models
{
    /// <summary>
    /// Representa um ativo de rede (Roteador, Switch, Servidor, etc.) dentro de uma organização.
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Identificador único do dispositivo.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nome amigável do dispositivo.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de hardware do dispositivo.
        /// </summary>
        public EDeviceTypes Type { get; set; }

        /// <summary>
        /// Endereço IP (opcional) para acesso ou gerência do dispositivo.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Identificador da organização proprietária.
        /// </summary>
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// Referência de navegação para a organização.
        /// </summary>
        public Organization Organization { get; set; } = default!;
    }
}
