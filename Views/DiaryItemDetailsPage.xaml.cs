using zorgApp.ViewModels;

namespace zorgApp.Views;

public partial class DiaryItemDetailsPage : ContentPage
{
    private readonly DiaryPageDetailsViewModel _viewModel;

    public DiaryItemDetailsPage(DiaryPageDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshCommand.Execute(null);
    }
}