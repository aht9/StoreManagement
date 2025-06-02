namespace StoreManagement.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow(ShellViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            
            if (viewModel != null)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
            
            Loaded += (s, e) => UpdateSidemenuVisualState(viewModel.IsSidemenuOpen);
            
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSidemenuOpen")
            {
                if (sender is ShellViewModel changedViewModel)
                {
                    UpdateSidemenuVisualState(changedViewModel.IsSidemenuOpen);
                }
                else if (DataContext is ShellViewModel dcViewModel)
                {
                    UpdateSidemenuVisualState(dcViewModel.IsSidemenuOpen);
                }
            }
        }

        private void UpdateSidemenuVisualState(bool isOpen)
        {
            if (this.LayoutRootGrid != null)
            {
                if (isOpen)
                {
                    VisualStateManager.GoToState(this.LayoutRootGrid, "SidemenuOpen", true);
                }
                else
                {
                    VisualStateManager.GoToState(this.LayoutRootGrid, "SidemenuClosed", true);
                }
            }
        }

        
        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is ShellViewModel viewModel)
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosed(e);
        }
    }
}