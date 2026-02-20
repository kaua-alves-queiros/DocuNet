using DocuNet.Web.Enumerators;
using MudBlazor;

namespace DocuNet.Web.Constants;

/// <summary>
/// Ícones SVG (Material Design) para cada tipo de dispositivo.
/// Utiliza as constantes de ícones do MudBlazor (MudBlazor.Icons.Material.Filled).
/// </summary>
public static class DeviceIcons
{
    /// <summary>
    /// Retorna o ícone SVG completo, unificado com os ícones do sistema, para uso na topologia.
    /// </summary>
    public static string GetIcon(EDeviceTypes type)
    {
        var iconContent = GetMudIcon(type);
        // Usamos o atributo fill direto para melhor compatibilidade com o btoa do JS e renderização em zoom
        return $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='white'>{iconContent}</svg>";
    }

    /// <summary>
    /// Retorna o ícone Material (string) do MudBlazor correspondente ao tipo de dispositivo.
    /// </summary>
    public static string GetMudIcon(EDeviceTypes type) => type switch
    {
        EDeviceTypes.Router => Icons.Material.Filled.Router,
        EDeviceTypes.Switch => Icons.Material.Filled.Hub,
        EDeviceTypes.Modem => Icons.Material.Filled.SettingsInputAntenna,
        EDeviceTypes.Server => Icons.Material.Filled.Dns,
        EDeviceTypes.PC => Icons.Material.Filled.DesktopWindows,
        EDeviceTypes.Notebook => Icons.Material.Filled.Laptop,
        EDeviceTypes.AccessPoint => Icons.Material.Filled.Wifi,
        EDeviceTypes.WifiRouter => Icons.Material.Filled.WifiTethering,
        EDeviceTypes.Printer => Icons.Material.Filled.Print,
        EDeviceTypes.Specs => Icons.Material.Filled.SettingsApplications,
        _ => Icons.Material.Filled.Devices
    };

    /// <summary>
    /// Retorna a cor correspondente ao tipo de dispositivo para diferenciação visual.
    /// </summary>
    public static Color GetColor(EDeviceTypes type) => type switch
    {
        EDeviceTypes.Router => Color.Primary,
        EDeviceTypes.Switch => Color.Secondary,
        EDeviceTypes.Modem => Color.Info,
        EDeviceTypes.Server => Color.Error,
        EDeviceTypes.PC => Color.Success,
        EDeviceTypes.Notebook => Color.Warning,
        EDeviceTypes.AccessPoint => Color.Tertiary,
        EDeviceTypes.WifiRouter => Color.Primary,
        EDeviceTypes.Printer => Color.Info,
        EDeviceTypes.Specs => Color.Default,
        _ => Color.Default
    };
}
