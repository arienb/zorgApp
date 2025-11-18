using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using zorgApp.Models;
using zorgApp.Services;

namespace zorgApp.ViewModels
{
    [QueryProperty(nameof(PatientId), nameof(PatientId))]
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public partial class DiaryPageDetailsViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty]
        private string patientId = string.Empty;

        [ObservableProperty]
        private string itemId = string.Empty;

        [ObservableProperty]
        private DiaryItem? diaryItem;

        [ObservableProperty]
        private bool isLoading;

        public DiaryPageDetailsViewModel(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        partial void OnItemIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(PatientId))
            {
                _ = LoadDiaryItemAsync();
            }
        }

        partial void OnPatientIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(ItemId))
            {
                _ = LoadDiaryItemAsync();
            }
        }

        private async Task LoadDiaryItemAsync()
        {
            IsLoading = true;

            try
            {
                var item = await _firebaseService.GetDiaryItemByIdAsync(PatientId, ItemId);

                if (item != null)
                {
                    DiaryItem = item;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Fout", "Dagboek item niet gevonden.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fout", $"Er is een fout opgetreden: {ex.Message}", "OK");
                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task EditItemAsync()
        {
            if (DiaryItem != null)
            {
                await Shell.Current.GoToAsync($"{nameof(Views.AddDiaryItemView)}?PatientId={PatientId}&ItemId={DiaryItem.Id}");
            }
        }

        [RelayCommand]
        private async Task DeleteItemAsync()
        {
            if (DiaryItem == null)
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Verwijderen", 
                "Weet je zeker dat je dit dagboek item wilt verwijderen?", 
                "Ja", 
                "Nee");

            if (!confirm)
                return;

            try
            {
                await _firebaseService.DeleteDiaryItemAsync(PatientId, DiaryItem.Id);
                await Shell.Current.DisplayAlert("Succes", "Dagboek item verwijderd!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fout", $"Kon item niet verwijderen: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task GoBackCommand()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
