using zorgApp.ViewModels;

namespace zorgApp.Views;

public partial class AddDiaryItemView : ContentPage
{
	public AddDiaryItemView(AddDiaryItemViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}