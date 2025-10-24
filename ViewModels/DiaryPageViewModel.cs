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
    public partial class DiaryPageViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty]
        private bool _isRefreshing;

        public ObservableCollection<DiaryItem> DiaryItems { get; set; }

        public DiaryPageViewModel(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
            DiaryItems = new ObservableCollection<DiaryItem>();

            _ = LoadDiaryItemsAsync();
        }

        [RelayCommand]
        private async Task AddItemAsync()
        {
            await Shell.Current.GoToAsync(nameof(Views.AddDiaryItemView));
        }

        [RelayCommand]
        private async Task LoadDiaryItemsAsync()
        {
            IsRefreshing = true;

            try
            {
                var items = await _firebaseService.GetDiaryItemsAsync();
                
                // Sorteer chronologisch met nieuwste bovenaan
                var sortedItems = items.OrderByDescending(i => i.Timestamp).ToList();

                DiaryItems.Clear();
                foreach (var item in sortedItems)
                {
                    DiaryItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                // Handle error - toon melding aan gebruiker
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
            
            // Navigeer naar details pagina met ItemId parameter (exact zoals QueryProperty naam!)
            await Shell.Current.GoToAsync($"{nameof(Views.DiaryPageDetailsView)}?ItemId={item.Id}");
        }
    }
}
