using DocuNet.Web.Enumerators;
using MudBlazor;

namespace DocuNet.Web.Constants;

/// <summary>
/// Provedor de ícones e cores para os tipos de conexões de rede.
/// </summary>
public static class ConnectionIcons
{
    /// <summary>
    /// Retorna o ícone Material correspondente ao tipo de conexão.
    /// </summary>
    public static string GetIcon(EConnectionTypes type) => type switch
    {
        EConnectionTypes.Ethernet => Icons.Material.Filled.SettingsEthernet,
        EConnectionTypes.Fiber => Icons.Material.Filled.FiberManualRecord,
        EConnectionTypes.Wireless => Icons.Material.Filled.Wifi,
        EConnectionTypes.Radio => Icons.Material.Filled.Sensors,
        EConnectionTypes.VPN => Icons.Material.Filled.VpnKey,
        EConnectionTypes.Serial => Icons.Material.Filled.SettingsInputHdmi,
        EConnectionTypes.Other => Icons.Material.Filled.Link,
        _ => Icons.Material.Filled.Link
    };

    /// <summary>
    /// Retorna a cor correspondente ao tipo de conexão para diferenciação visual.
    /// </summary>
    public static Color GetColor(EConnectionTypes type) => type switch
    {
        EConnectionTypes.Ethernet => Color.Primary,
        EConnectionTypes.Fiber => Color.Info,
        EConnectionTypes.Wireless => Color.Success,
        EConnectionTypes.Radio => Color.Warning,
        EConnectionTypes.VPN => Color.Error,
        EConnectionTypes.Serial => Color.Secondary,
        EConnectionTypes.Other => Color.Default,
        _ => Color.Default
    };
}
