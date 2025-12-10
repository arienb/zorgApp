using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace zorgApp.ViewModels
{
    public partial class StartupSelectionViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isInfoPopupVisible;

        [RelayCommand]
        private async Task Patient()
        {
            await Shell.Current.GoToAsync("PatientLoginPage");
        }

        [RelayCommand]
        private async Task Nurse()
        {
            await Shell.Current.GoToAsync("NurseLoginPage");
        }

        [RelayCommand]
        private void ShowInfo()
        {
            IsInfoPopupVisible = true;
        }

        [RelayCommand]
        private void CloseInfo()
        {
            IsInfoPopupVisible = false;
        }
    }
}
