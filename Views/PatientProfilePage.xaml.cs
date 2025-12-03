using zorgApp.ViewModels;

namespace zorgApp.Views;

public partial class PatientProfilePage : ContentPage
{
	public PatientProfilePage(PatientProfileViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}