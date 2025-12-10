using zorgApp.ViewModels;

namespace zorgApp.Views;

public partial class PatientSelectionPage : ContentPage
{
	public PatientSelectionPage(PatientSelectionScreenViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}