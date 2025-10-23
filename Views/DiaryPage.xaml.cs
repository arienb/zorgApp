using zorgApp.ViewModels;

namespace zorgApp.Views
{
    public partial class DiaryPage : ContentPage
    {
        public DiaryPage(DiaryPageViewModel viewModel)
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
    }
}
