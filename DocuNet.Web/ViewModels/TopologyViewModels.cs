using System;

namespace DocuNet.Web.ViewModels
{
    public class SelectedElement
    {
        public string? Id { get; set; }
        public string? Label { get; set; }
        public string? Ip { get; set; }
        public string? SourcePort { get; set; }
        public string? TargetPort { get; set; }
    }
}
