using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using zorgApp.Models;
using zorgApp.Services;

namespace zorgApp.ViewModels
{
    public partial class AddDiaryItemViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;
        private FileResult? _selectedImageResult;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private DateTime _timestamp = DateTime.Now;

        [ObservableProperty]
        private TimeSpan _time = DateTime.Now.TimeOfDay;

        [ObservableProperty]
        private string _createdBy = string.Empty;

        [ObservableProperty]
        private bool _isSaving;

        [ObservableProperty]
        private ImageSource _image;

        public AddDiaryItemViewModel(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }


        // ------------- diaryItem saven -------------
        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                await Shell.Current.DisplayAlert("Validatie", "Titel is verplicht", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                await Shell.Current.DisplayAlert("Validatie", "Beschrijving is verplicht", "OK");
                return;
            }

            IsSaving = true;

            string? imageUrl = null;
            if (_selectedImageResult != null)
            {
                using var stream = await _selectedImageResult.OpenReadAsync();
                imageUrl = await _firebaseService.UploadImageAsync(stream, $"{Guid.NewGuid()}.jpg");
            }
            

            try
            {
                // Combineer datum en tijd
                var combinedDateTime = Timestamp.Date + Time;

                var newItem = new DiaryItem
                {
                    Title = Title,
                    Description = Description,
                    ImageUrl = imageUrl,
                    Timestamp = combinedDateTime,
                    CreatedBy = CreatedBy
                };

                await _firebaseService.AddDiaryItemAsync(newItem);

                await Shell.Current.DisplayAlert("Succes", "Dagboek item toegevoegd!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fout", $"Kon item niet toevoegen: {ex.Message}", "OK");
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private async Task PickImage() 
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync();

                if (result != null) {
                    _selectedImageResult = result;
                    Image = ImageSource.FromStream(() => result.OpenReadAsync().Result);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fout", $"kan de foto niet laden {ex.Message}", "Oke");
            }
        }

        private bool CanSave() => !IsSaving;

        partial void OnIsSavingChanged(bool value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
