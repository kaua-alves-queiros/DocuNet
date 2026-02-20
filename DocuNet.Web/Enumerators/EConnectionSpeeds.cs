using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Enumerators;

/// <summary>
/// Define as velocidades padrão de conexão de rede.
/// </summary>
public enum EConnectionSpeeds
{
    [Display(Name = "10 Mbps (Ethernet)")]
    Ethernet10M,

    [Display(Name = "100 Mbps (Fast Ethernet)")]
    FastEthernet100M,

    [Display(Name = "1 Gbps (Gigabit)")]
    Gigabit1G,

    [Display(Name = "2.5 Gbps")]
    MultiGigabit2_5G,

    [Display(Name = "5 Gbps")]
    MultiGigabit5G,

    [Display(Name = "10 Gbps")]
    TenGigabit10G,

    [Display(Name = "25 Gbps")]
    TwentyFiveGigabit25G,

    [Display(Name = "40 Gbps")]
    FortyGigabit40G,

    [Display(Name = "50 Gbps")]
    FiftyGigabit50G,

    [Display(Name = "100 Gbps")]
    HundredGigabit100G,

    [Display(Name = "Outra / Customizada")]
    Other
}
