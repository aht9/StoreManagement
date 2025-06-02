namespace StoreManagement.UI.Services;

public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }
    event Action<ViewModelBase> CurrentViewModelChanged;
    void NavigateTo(Type viewModelType);
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
}