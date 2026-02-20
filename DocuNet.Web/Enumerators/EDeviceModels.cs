using System.ComponentModel.DataAnnotations;

namespace DocuNet.Web.Enumerators
{
    public enum EDeviceModels
    {
        [Display(Name = "Roteador")]
        Router,

        [Display(Name = "Switch")]
        Switch,

        [Display(Name = "Modem")]
        Modem,

        [Display(Name = "Servidor")]
        Server,

        [Display(Name = "Desktop / PC")]
        PC,

        [Display(Name = "Notebook")]
        Notebook,

        [Display(Name = "Ponto de Acesso (AP)")]
        AccessPoint,

        [Display(Name = "Roteador Wi-Fi")]
        WifiRouter,

        [Display(Name = "Especificações / Outros")]
        Specs
    }
}
