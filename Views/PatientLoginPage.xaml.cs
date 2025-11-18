using zorgApp.ViewModels;

namespace zorgApp.Views;

public partial class PatientLoginPage : ContentPage
{
    public PatientLoginPage(PatientLoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}