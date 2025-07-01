namespace StoreManagement.WPF.ViewModels.Invoicing;

public class ConfirmationDialogViewModel(string title, string message)
{
    public string Title { get; } = title;
    public string Message { get; } = message;
}