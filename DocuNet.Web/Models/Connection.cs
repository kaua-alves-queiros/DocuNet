using DocuNet.Web.Enumerators;

namespace DocuNet.Web.Models;

/// <summary>
/// Representa uma conexão física ou lógica entre dois dispositivos.
/// </summary>
public class Connection
{
    public Guid Id { get; set; }

    /// <summary>
    /// ID do primeiro dispositivo (Origem / Ponta A).
    /// </summary>
    public Guid SourceDeviceId { get; set; }
    public Device SourceDevice { get; set; } = null!;

    /// <summary>
    /// Nome da interface no dispositivo de origem (ex: "eth0", "Gi0/1").
    /// </summary>
    public string? SourceInterface { get; set; }

    /// <summary>
    /// ID do segundo dispositivo (Destino / Ponta B).
    /// </summary>
    public Guid DestinationDeviceId { get; set; }
    public Device DestinationDevice { get; set; } = null!;

    /// <summary>
    /// Nome da interface no dispositivo de destino (ex: "sfp-sfpplus1").
    /// </summary>
    public string? DestinationInterface { get; set; }

    /// <summary>
    /// Tipo de conexão (Ethernet, Fiber, Wireless, etc).
    /// </summary>
    public EConnectionTypes Type { get; set; }

    /// <summary>
    /// Velocidade da conexão (ex: "1 Gbps", "100 Mbps").
    /// </summary>
    public string? Speed { get; set; }

    /// <summary>
    /// ID da organização à qual esta conexão pertence.
    /// </summary>
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
}
