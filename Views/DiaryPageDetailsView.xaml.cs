using zorgApp.ViewModels;

namespace zorgApp.Views;

public partial class DiaryPageDetailsView : ContentPage
{
    public DiaryPageDetailsView(DiaryPageDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}