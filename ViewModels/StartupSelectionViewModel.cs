using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace zorgApp.ViewModels
{
    public partial class StartupSelectionViewModel : ObservableObject
    {
        [RelayCommand]
        private async Task Patient()
        {
            await Shell.Current.GoToAsync("DiaryPage");
        }

        [RelayCommand]
        private async Task Nurse()
        {
            await Shell.Current.DisplayAlert("Info", "Nurse functionality not implemented.", "OK");
        }
    }
}
