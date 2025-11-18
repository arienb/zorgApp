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
            await Shell.Current.GoToAsync("PatientLoginPage");
        }

        [RelayCommand]
        private async Task Nurse()
        {
            string password = await Shell.Current.DisplayPromptAsync(
                "Verpleegkundige Login",
                "Voer uw wachtwoord in:",
                keyboard: Keyboard.Numeric,
                maxLength: 4
            );

            if (password == "1234")
            {
                await Shell.Current.GoToAsync("PatientSelectionScreen");
            }
            else if (!string.IsNullOrEmpty(password))
            {
                await Shell.Current.DisplayAlert(
                    "Toegang Geweigerd",
                    "Ongeldig wachtwoord. Probeer het opnieuw.",
                    "OK"
                );
            }
        }
    }
}
