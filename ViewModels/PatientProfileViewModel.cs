using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zorgApp.Models;
using zorgApp.Services;
using zorgApp.Views;

namespace zorgApp.ViewModels
{
    [QueryProperty(nameof(Patient), nameof(Patient))]
    public partial class PatientProfileViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty]
        private Patient patient;

        [ObservableProperty] private bool isEditingCallName;
        [ObservableProperty] private bool isEditingHobbies;
        [ObservableProperty] private bool isEditingFood;
        [ObservableProperty] private bool isEditingFilm;
        [ObservableProperty] private bool isEditingMusic;
        [ObservableProperty] private bool isEditingWork;

        public PatientProfileViewModel(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        [RelayCommand]
        private async Task SaveProfileAsync()
        {
            try
            {
                await _firebaseService.UpdatePatientProfileAsync(Patient.FirebaseId, Patient);
                await Shell.Current.DisplayAlert("Succes", "Profiel opgeslagen", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveProfileAsync Error: {ex.Message}");
                await Shell.Current.DisplayAlert("Fout", "Opslaan mislukt", "OK");
            }
        }
    }
}
