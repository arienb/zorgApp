using zorgApp.ViewModels;

namespace zorgApp.Views;

public partial class PatientSelectionScreen : ContentPage
{
	public PatientSelectionScreen(PatientSelectionScreenViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}