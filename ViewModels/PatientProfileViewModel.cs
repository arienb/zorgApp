using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading.Tasks;
using zorgApp.Models;
using zorgApp.Services;

namespace zorgApp.ViewModels
{
    [QueryProperty(nameof(Patient), nameof(Patient))]
    public partial class PatientProfileViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty]
        private Patient patient;

        [ObservableProperty] 
        private bool hasProfileImage;

        [ObservableProperty] 
        private bool isUploadingImage;

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

        partial void OnPatientChanged(Patient value)
        {
            if (value != null)
            {
                UpdateProfileImageStatus();
            }
        }

        private void UpdateProfileImageStatus()
        {
            HasProfileImage = !string.IsNullOrEmpty(Patient?.ProfileImageUrl) && 
                             Patient.ProfileImageUrl != "profile.png";
        }

        [RelayCommand]
        private async Task SelectProfileImageAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Selecteer een profielfoto"
                });

                if (result != null)
                {
                    IsUploadingImage = true;

                    // Open the selected image stream
                    var imageStream = await result.OpenReadAsync();
                    var fileName = $"profile_{Patient.FirebaseId}_{DateTime.Now:yyyyMMddHHmmss}.jpg";

                    // Create a memory stream to keep the image data
                    var memoryStream = new MemoryStream();
                    await imageStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    imageStream.Dispose();

                    // Upload image immediately and update patient
                    var imageUrl = await _firebaseService.UploadImageAsync(memoryStream, $"profiles/{fileName}");
                    
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // Update patient object
                        Patient.ProfileImageUrl = imageUrl;
                        
                        // Save to Firebase immediately
                        await _firebaseService.UpdatePatientProfileAsync(Patient.FirebaseId, Patient, null, null);
                        
                        HasProfileImage = true;
                        OnPropertyChanged(nameof(Patient));
                        
                        await Shell.Current.DisplayAlert("Succes", "Profielfoto opgeslagen", "OK");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Fout", "Kon afbeelding niet uploaden", "OK");
                    }

                    memoryStream.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SelectProfileImage Error: {ex.Message}");
                await Shell.Current.DisplayAlert("Fout", "Kon afbeelding niet selecteren", "OK");
            }
            finally
            {
                IsUploadingImage = false;
            }
        }

        [RelayCommand]
        private async Task SaveProfileAsync()
        {
            try
            {
                await _firebaseService.UpdatePatientProfileAsync(
                    Patient.FirebaseId,
                    Patient,
                    null,
                    null);

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
