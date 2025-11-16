using zorgApp.ViewModels;

namespace zorgApp.Views
{
    public partial class StartupSelectionPage : ContentPage
    {
        public StartupSelectionPage(StartupSelectionViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
