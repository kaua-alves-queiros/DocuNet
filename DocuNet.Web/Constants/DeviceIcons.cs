using DocuNet.Web.Enumerators;
using MudBlazor;

namespace DocuNet.Web.Constants;

/// <summary>
/// Ícones SVG (Material Design) para cada tipo de dispositivo.
/// Utiliza as constantes de ícones do MudBlazor (MudBlazor.Icons.Material.Filled).
/// </summary>
public static class DeviceIcons
{
    // SVG paths dos ícones Material Design para cada tipo de dispositivo
    public const string Router = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M0 0h24v24H0z' fill='none'/><path d='M10.59 1.41L9.17 2.83l2.17 2.17H5.5C4.12 5 3 6.12 3 7.5V11h2V7.5c0-.28.22-.5.5-.5h5.84L9.17 9.17l1.41 1.41L14 7.17 10.59 1.41zM19 13h-2v3.5c0 .28-.22.5-.5.5H10.66l2.17-2.17-1.41-1.41L8 16.83l3.41 6.76 1.41-1.41-2.17-2.17H18.5c1.38 0 2.5-1.12 2.5-2.5V13zM5 13H3v1c0 2.97 2.16 5.43 5 5.91V22h2v-3.09c1.28-.31 2.42-.97 3.32-1.88l-1.42-1.42C11.18 16.53 9.69 17 8 17c-2.8 0-5-2.2-5-5v-4H1v4c0 3.52 2.03 6.58 5 8.07V22h2v-3.09A9.007 9.007 0 0 0 17 13v-1h-2v1c0 2.76-2.24 5-5 5s-5-2.24-5-5v-1z'/></svg>";

    public const string Switch = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M0 0h24v24H0z' fill='none'/><path d='M15 3H5c-1.1 0-2 .9-2 2v4c0 1.1.9 2 2 2h7l4-4V3zm2 6l-2 2 2 2 2-2-2-2zm-4 4H5c-1.1 0-2 .9-2 2v4c0 1.1.9 2 2 2h10c1.1 0 2-.9 2-2V16c0-1.1-.9-2-2-2z'/></svg>";

    public const string Modem = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M0 0h24v24H0z' fill='none'/><path d='M1 9l2 2c2.88-2.88 6.79-4.08 10.53-3.62l1.19-2.44C9.88 4.2 4.96 5.75 1 9zm20.64-.64l-1.19 2.44C21.87 12.23 23 14.97 23 18h3C26 14.07 24.38 10.51 21.64 8.36zM21 18c0-2.76-1.12-5.26-2.93-7.07l-1.41 1.41C18.09 13.86 19 15.83 19 18h2zM6.34 11.34L4.93 12.76C6.73 14.56 7.85 17.07 8 19.8L9.99 20c.23-3.41-1.07-6.73-3.65-8.66z'/></svg>";

    public const string Server = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M0 0h24v24H0z' fill='none'/><path d='M20 2H4v6h16V2zm-2 4h-2V4h2v2zM4 14h16v-4H4v4zm2-3h2v2H6v-2zm0 7h16v-4H4v4zm2-3h2v2H6v-2z'/></svg>";

    public const string PC = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M0 0h24v24H0z' fill='none'/><path d='M21 2H3c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h7l-2 3v1h8v-1l-2-3h7c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 12H3V4h18v10z'/></svg>";

    public const string Notebook = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M0 0h24v24H0z' fill='none'/><path d='M20 18c1.1 0 1.99-.9 1.99-2L22 6a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2H0v2h24v-2h-4zM4 6h16v10H4V6z'/></svg>";

    public const string AccessPoint = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M0 0h24v24H0z' fill='none'/><path d='M12 3C6.95 3 3.15 5.85 1 10h2.05C5.2 7 8.4 5 12 5s6.8 2 8.95 5H23C20.85 5.85 17.05 3 12 3zm0 4c-2.76 0-5 2.24-5 5s2.24 5 5 5 5-2.24 5-5-2.24-5-5-5zm0 8c-1.66 0-3-1.34-3-3s1.34-3 3-3 3 1.34 3 3-1.34 3-3 3z'/></svg>";

    public const string WifiRouter = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M0 0h24v24H0z' fill='none'/><path d='M1 9l2 2c2.88-2.88 6.79-4.08 10.53-3.62l1.19-2.44C9.88 4.2 4.96 5.75 1 9zm8 8l3 3 3-3a4.237 4.237 0 0 0-6 0zm-4-4 2 2a7.074 7.074 0 0 1 10 0l2-2C15.14 9.14 8.86 9.14 5 13zm16.79-9.37-1.19 2.44c1.77.87 3.38 2.07 4.79 3.55L23 9C21.41 7.39 19.2 6.01 21.79 3.63z'/></svg>";

    public const string Specs = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M0 0h24v24H0z' fill='none'/><path d='M19.14 12.94c.04-.3.06-.61.06-.94 0-.32-.02-.64-.07-.94l2.03-1.58a.49.49 0 0 0 .12-.61l-1.92-3.32a.488.488 0 0 0-.59-.22l-2.39.96c-.5-.38-1.03-.7-1.62-.94l-.36-2.54a.484.484 0 0 0-.48-.41h-3.84c-.24 0-.43.17-.47.41l-.36 2.54c-.59.24-1.13.57-1.62.94l-2.39-.96a.488.488 0 0 0-.59.22L2.74 8.87a.48.48 0 0 0 .12.61l2.03 1.58c-.05.3-.09.63-.09.94s.02.64.07.94l-2.03 1.58a.49.49 0 0 0-.12.61l1.92 3.32c.12.22.37.29.59.22l2.39-.96c.5.38 1.03.7 1.62.94l.36 2.54c.05.24.24.41.48.41h3.84c.24 0 .44-.17.47-.41l.36-2.54c.59-.24 1.13-.56 1.62-.94l2.39.96c.22.08.47 0 .59-.22l1.92-3.32a.48.48 0 0 0-.12-.61l-2.01-1.58zM12 15.6c-1.98 0-3.6-1.62-3.6-3.6s1.62-3.6 3.6-3.6 3.6 1.62 3.6 3.6-1.62 3.6-3.6 3.6z'/></svg>";

    /// <summary>
    /// Retorna o ícone SVG correspondente ao tipo de dispositivo.
    /// </summary>
    public static string GetIcon(EDeviceTypes type) => type switch
    {
        EDeviceTypes.Router => Router,
        EDeviceTypes.Switch => Switch,
        EDeviceTypes.Modem => Modem,
        EDeviceTypes.Server => Server,
        EDeviceTypes.PC => PC,
        EDeviceTypes.Notebook => Notebook,
        EDeviceTypes.AccessPoint => AccessPoint,
        EDeviceTypes.WifiRouter => WifiRouter,
        EDeviceTypes.Specs => Specs,
        _ => Server
    };

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
        EDeviceTypes.Specs => Color.Default,
        _ => Color.Default
    };
}
