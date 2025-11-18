using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using zorgApp.Models;
using zorgApp.Services;

namespace zorgApp.ViewModels
{
    [QueryProperty(nameof(PatientId), nameof(PatientId))]
    public partial class DiaryPageViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty]
        private string patientId = string.Empty;

        [ObservableProperty]
        private bool isRefreshing;

        public ObservableCollection<DiaryItem> DiaryItems { get; set; }

        public DiaryPageViewModel(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
            DiaryItems = new ObservableCollection<DiaryItem>();
        }

        partial void OnPatientIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _ = LoadDiaryItemsAsync();
            }
        }

        [RelayCommand]
        private async Task AddItemAsync()
        {
            if (string.IsNullOrEmpty(PatientId))
            {
                await Shell.Current.DisplayAlert("Fout", "Geen patiënt geselecteerd", "OK");
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(Views.AddDiaryItemView)}?PatientId={PatientId}");
        }

        [RelayCommand]
        private async Task LoadDiaryItemsAsync()
        {
            if (string.IsNullOrEmpty(PatientId))
                return;

            IsRefreshing = true;

            try
            {
                var items = await _firebaseService.GetDiaryItemsAsync(PatientId);
                
                var sortedItems = items.OrderByDescending(i => i.Timestamp).ToList();

                DiaryItems.Clear();
                foreach (var item in sortedItems)
                {
                    DiaryItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fout", $"Kon dagboek items niet laden: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task ItemTappedAsync(DiaryItem item)
        {
            if (item == null)
                return;

            System.Diagnostics.Debug.WriteLine($"ItemTapped - Navigating with ID: {item.Id}");
            
            await Shell.Current.GoToAsync($"{nameof(Views.DiaryPageDetailsView)}?PatientId={PatientId}&ItemId={item.Id}");
        }

        [RelayCommand]
        private async Task GoBackCommand()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
