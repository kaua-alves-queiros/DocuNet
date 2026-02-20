using DocuNet.Web.Dtos.Organization;
using DocuNet.Web.Services;

namespace DocuNet.Web.States;

public class OrganizationState(OrganizationService organizationService)
{
    private readonly OrganizationService _organizationService = organizationService;

    public List<OrganizationSummaryDto> AvailableOrganizations { get; private set; } = [];
    public OrganizationSummaryDto? CurrentOrganization { get; private set; }

    public event Action? OnChange;

    public async Task InitializeAsync(Guid userId)
    {
        var result = await _organizationService.GetAvailableOrganizationsAsync(userId);
        if (result.Success && result.Data != null)
        {
            AvailableOrganizations = result.Data;

            // Se a organização atual não estiver na lista (ex: foi removida ou permissão mudou), limpa seleção
            if (CurrentOrganization != null && !AvailableOrganizations.Any(o => o.Id == CurrentOrganization.Id))
            {
                CurrentOrganization = null;
            }

            // Se nenhuma estiver selecionada e houver disponíveis, seleciona a primeira por padrão
            if (CurrentOrganization == null && AvailableOrganizations.Count > 0)
            {
                CurrentOrganization = AvailableOrganizations.First();
            }

            NotifyStateChanged();
        }
    }

    public void SetOrganization(OrganizationSummaryDto organization)
    {
        CurrentOrganization = organization;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
