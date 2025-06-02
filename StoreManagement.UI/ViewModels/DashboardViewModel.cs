namespace StoreManagement.UI.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    public DashboardViewModel(ILogger logger)
    {
        Title = "Dashboard";
        logger.ForContext<DashboardViewModel>().Information("DashboardViewModel initialized.");
    }
}