using zorgApp.ViewModels;

namespace zorgApp.Views
{
    public partial class NurseLoginPage : ContentPage
    {
        public NurseLoginPage(NurseLoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}