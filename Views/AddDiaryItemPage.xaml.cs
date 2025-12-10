using zorgApp.ViewModels;

namespace zorgApp.Views;

public partial class AddDiaryItemPage : ContentPage
{
	public AddDiaryItemPage(AddDiaryItemViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}