using zorgApp.ViewModels;

namespace zorgApp.Views
{
    public partial class DiaryItemsPage : ContentPage
    {
        public DiaryItemsPage(DiaryPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Refresh de lijst wanneer we terug navigeren naar deze pagina
            if (BindingContext is DiaryPageViewModel viewModel)
            {
                viewModel.LoadDiaryItemsCommand.Execute(null);
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}
