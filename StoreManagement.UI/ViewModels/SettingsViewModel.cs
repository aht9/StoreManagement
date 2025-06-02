namespace StoreManagement.UI.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel(ILogger logger)
    {
        Title = "Settings";
        logger.ForContext<SettingsViewModel>().Information("SettingsViewModel initialized.");
    }
}