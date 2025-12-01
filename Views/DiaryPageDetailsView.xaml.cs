using zorgApp.ViewModels;

namespace zorgApp.Views;

public partial class DiaryPageDetailsView : ContentPage
{
    private readonly DiaryPageDetailsViewModel _viewModel;

    public DiaryPageDetailsView(DiaryPageDetailsViewModel viewModel)
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