using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Enumerators;

/// <summary>
/// Tipos de conexões físicas ou lógicas entre dispositivos de rede.
/// </summary>
public enum EConnectionTypes
{
    [Display(Name = "Ethernet (Cabo)")]
    Ethernet,

    [Display(Name = "Fibra Óptica")]
    Fiber,

    [Display(Name = "Wireless (Wi-Fi)")]
    Wireless,

    [Display(Name = "Rádio (Point-to-Point)")]
    Radio,

    [Display(Name = "VPN / Túnel")]
    VPN,

    [Display(Name = "Serial / Console")]
    Serial,

    [Display(Name = "Outro")]
    Other
}
