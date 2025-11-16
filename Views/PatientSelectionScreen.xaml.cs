using zorgApp.ViewModels;

namespace zorgApp.Views;

public partial class PatientSelectionScreen : ContentPage
{
	public PatientSelectionScreen()
	{
		InitializeComponent();
        BindingContext = new PatientSelectionScreenViewModel();
    }
}