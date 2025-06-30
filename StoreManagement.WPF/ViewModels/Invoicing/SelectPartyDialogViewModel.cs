namespace StoreManagement.WPF.ViewModels.Invoicing;

public partial class SelectPartyDialogViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly PartyTypeToQuery _partyType;

    [ObservableProperty]
    private string _searchQuery;

    [ObservableProperty]
    private ObservableCollection<PartyDto> _parties;

    [ObservableProperty]
    private PartyDto _selectedParty;

    public string Title => _partyType == PartyTypeToQuery.Customers ? "انتخاب مشتری" : "انتخاب فروشگاه";

    public SelectPartyDialogViewModel(IMediator mediator, PartyTypeToQuery partyType)
    {
        _mediator = mediator;
        _partyType = partyType;
        Task.Run(Search);
    }

    [RelayCommand]
    private async Task Search()
    {
        var query = new GetPartiesQuery
        {
            Type = _partyType,
            SearchTerm = this.SearchQuery
        };
        var result = await _mediator.Send(query);
        Parties = new ObservableCollection<PartyDto>(result);
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (SearchCommand.CanExecute(null))
        {
            SearchCommand.Execute(null);
        }
    }

    public bool CanSelectParty() => SelectedParty != null;

}