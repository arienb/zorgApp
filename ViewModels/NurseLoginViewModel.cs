using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using zorgApp.Models;
using zorgApp.Services;

namespace zorgApp.ViewModels
{
    public partial class NurseLoginViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;
        private const string AdminPassword = "admin";

        [ObservableProperty]
        private string departmentName = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isCreatingDepartment;

        [ObservableProperty]
        private bool isPasswordVisible;

        public NurseLoginViewModel(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(DepartmentName))
            {
                await Shell.Current.DisplayAlert("Fout", "Voer een afdelingsnaam in", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Fout", "Voer een wachtwoord in", "OK");
                return;
            }

            IsLoading = true;

            try
            {
                var isValid = await _firebaseService.ValidateDepartmentCredentialsAsync(
                    DepartmentName.Trim(), 
                    Password.Trim()
                );

                if (isValid)
                {
                    // Store department name for session
                    Preferences.Set("CurrentDepartment", DepartmentName.Trim());
                    
                    // Navigate to PatientSelectionPage (relative route)
                    await Shell.Current.GoToAsync("PatientSelectionPage");
                }
                else
                {
                    await Shell.Current.DisplayAlert(
                        "Toegang Geweigerd", 
                        "Ongeldige afdeling of wachtwoord.", 
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login Error: {ex}");
                await Shell.Current.DisplayAlert("Fout", $"Er is een fout opgetreden: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task CreateDepartmentAsync()
        {
            try
            {
                // Vraag admin wachtwoord
                var adminPasswordInput = await Shell.Current.DisplayPromptAsync(
                    "Beheerderstoegang vereist",
                    "Voer het beheerderswachtwoord in om een nieuwe afdeling aan te maken:",
                    accept: "OK",
                    cancel: "Annuleren",
                    placeholder: "Wachtwoord",
                    maxLength: 20,
                    keyboard: Keyboard.Default
                );

                // Check if cancelled
                if (string.IsNullOrEmpty(adminPasswordInput))
                {
                    return;
                }

                // Validate admin password
                if (adminPasswordInput != AdminPassword)
                {
                    await Shell.Current.DisplayAlert(
                        "Toegang Geweigerd",
                        "Ongeldig beheerderswachtwoord.",
                        "OK"
                    );
                    return;
                }

                // Proceed with department creation
                if (string.IsNullOrWhiteSpace(DepartmentName))
                {
                    await Shell.Current.DisplayAlert("Fout", "Voer een afdelingsnaam in", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Fout", "Voer een wachtwoord in voor de nieuwe afdeling", "OK");
                    return;
                }

                if (Password.Length < 4)
                {
                    await Shell.Current.DisplayAlert("Fout", "Wachtwoord moet minimaal 4 tekens bevatten", "OK");
                    return;
                }

                IsCreatingDepartment = true;

                // Check if department already exists
                var existing = await _firebaseService.GetDepartmentByNameAsync(DepartmentName.Trim());
                if (existing != null)
                {
                    await Shell.Current.DisplayAlert(
                        "Fout", 
                        "Deze afdeling bestaat al. Gebruik login.", 
                        "OK"
                    );
                    return;
                }

                var nurse = new Nurse
                {
                    DepartmentName = DepartmentName.Trim(),
                    Password = Password.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                System.Diagnostics.Debug.WriteLine($"Creating department: {nurse.DepartmentName}");

                var id = await _firebaseService.AddDepartmentAsync(nurse);

                System.Diagnostics.Debug.WriteLine($"Department created with ID: {id}");

                if (!string.IsNullOrEmpty(id))
                {
                    await Shell.Current.DisplayAlert(
                        "Afdeling Aangemaakt", 
                        $"Afdeling '{DepartmentName}' is succesvol aangemaakt!\n\nU kunt nu inloggen met dit wachtwoord.", 
                        "OK"
                    );

                    // Keep the password filled for immediate login
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateDepartment Error: {ex}");
                await Shell.Current.DisplayAlert(
                    "Fout", 
                    $"Er is een fout opgetreden bij het aanmaken van de afdeling:\n\n{ex.Message}", 
                    "OK"
                );
            }
            finally
            {
                IsCreatingDepartment = false;
            }
        }

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }
        [RelayCommand]
        private async Task Back()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}