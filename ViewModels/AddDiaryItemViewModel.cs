using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using zorgApp.Models;
using zorgApp.Services;

namespace zorgApp.ViewModels
{
    [QueryProperty(nameof(PatientId), nameof(PatientId))]
    [QueryProperty(nameof(DiaryItemId), nameof(DiaryItemId))]
    public partial class AddDiaryItemViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;
        private FileResult? _selectedImageResult;

        [ObservableProperty]
        private string patientId = string.Empty;

        [ObservableProperty]
        private string? diaryItemId;

        [ObservableProperty]
        private bool isEditMode;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private DateTime timestamp = DateTime.Now;

        [ObservableProperty]
        private TimeSpan time = DateTime.Now.TimeOfDay;

        [ObservableProperty]
        private string createdBy = string.Empty;

        [ObservableProperty]
        private bool isSaving;

        [ObservableProperty]
        private ImageSource? image;

        [ObservableProperty]
        private string? existingImageUrl;

        public AddDiaryItemViewModel(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        partial void OnDiaryItemIdChanged(string? value)
        {
            IsEditMode = !string.IsNullOrEmpty(value);
            if (IsEditMode)
            {
                _ = LoadDiaryItemAsync();
            }
        }

        private async Task LoadDiaryItemAsync()
        {
            if (string.IsNullOrEmpty(DiaryItemId) || string.IsNullOrEmpty(PatientId))
                return;

            try
            {
                var diaryItems = await _firebaseService.GetDiaryItemsAsync(PatientId);
                var item = diaryItems.FirstOrDefault(d => d.Id == DiaryItemId);

                if (item != null)
                {
                    Title = item.Title;
                    Description = item.Description;
                    Timestamp = item.Timestamp.Date;
                    Time = item.Timestamp.TimeOfDay;
                    CreatedBy = item.CreatedBy;
                    ExistingImageUrl = item.ImageUrl;

                    if (!string.IsNullOrEmpty(item.ImageUrl))
                    {
                        Image = ImageSource.FromUri(new Uri(item.ImageUrl));
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fout", $"Kon item niet laden: {ex.Message}", "OK");
            }
        }

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

            if (string.IsNullOrEmpty(PatientId))
            {
                await Shell.Current.DisplayAlert("Fout", "Geen patiënt geselecteerd", "OK");
                return;
            }

            IsSaving = true;

            try
            {
                string? imageUrl = ExistingImageUrl;
                
                // Upload new image if selected
                if (_selectedImageResult != null)
                {
                    using var stream = await _selectedImageResult.OpenReadAsync();
                    imageUrl = await _firebaseService.UploadImageAsync(stream, $"{Guid.NewGuid()}.jpg");
                }

                var combinedDateTime = Timestamp.Date + Time;

                if (IsEditMode && !string.IsNullOrEmpty(DiaryItemId))
                {
                    // Update existing item
                    var updatedItem = new DiaryItem
                    {
                        Id = DiaryItemId,
                        PatientId = PatientId,
                        Title = Title,
                        Description = Description,
                        ImageUrl = imageUrl,
                        Timestamp = combinedDateTime,
                        CreatedBy = CreatedBy
                    };

                    await _firebaseService.UpdateDiaryItemAsync(PatientId, updatedItem);
                    await Shell.Current.DisplayAlert("Succes", "Dagboek item bijgewerkt!", "OK");
                }
                else
                {
                    // Create new item
                    var newItem = new DiaryItem
                    {
                        PatientId = PatientId,
                        Title = Title,
                        Description = Description,
                        ImageUrl = imageUrl,
                        Timestamp = combinedDateTime,
                        CreatedBy = CreatedBy
                    };

                    await _firebaseService.AddDiaryItemAsync(PatientId, newItem);
                    await Shell.Current.DisplayAlert("Succes", "Dagboek item toegevoegd!", "OK");
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fout", $"Kon item niet opslaan: {ex.Message}", "OK");
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

                if (result != null) 
                {
                    _selectedImageResult = result;
                    Image = ImageSource.FromStream(() => result.OpenReadAsync().Result);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fout", $"Kan de foto niet laden: {ex.Message}", "OK");
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
