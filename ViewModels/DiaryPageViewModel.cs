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
        private string patientName = string.Empty;

        [ObservableProperty]
        private Notification? notification;

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
                _ = LoadPatientAndDiaryItemsAsync();
            }
        }

        private async Task LoadPatientAndDiaryItemsAsync()
        {
            await LoadPatientNameAsync();
            await LoadDiaryItemsAsync();
            Notification = await _firebaseService.GetNotificationAsync(PatientId);
        }

        private async Task LoadPatientNameAsync()
        {
            try
            {
                var patients = await _firebaseService.GetPatientsAsync();
                var patient = patients.FirstOrDefault(p => p.FirebaseId == PatientId);
                
                if (patient != null)
                {
                    PatientName = patient.Name;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading patient name: {ex.Message}");
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
        private async Task NavigateToProfileAsync() 
        {
            if (string.IsNullOrEmpty(patientId)) 
            {
                await Shell.Current.DisplayAlert("Fout", "kan huidige patiënt niet vinden", "OK");
                return;
            }

            var patients = await _firebaseService.GetPatientsAsync();
            var patient = patients.FirstOrDefault(p => p.FirebaseId == PatientId);
            if (patient is null)
            {
                await Shell.Current.DisplayAlert("Fout", "Patiënt niet gevonden", "OK");
                return;
            }

            await Shell.Current.GoToAsync(nameof(Views.PatientProfilePage), new Dictionary<string, object>
            {
                ["Patient"] = patient
            });
        }

        [RelayCommand]
        private async Task GoBackCommand()
        {
            await Shell.Current.GoToAsync("..");
        }

        //------- enkel voor verpleging -------//
        [RelayCommand]
        private async Task AddOrReplaceNotificationAsync()
        {
            if (string.IsNullOrEmpty(PatientId))
            {
                await Shell.Current.DisplayAlert("Fout", "Geen patiënt geselecteerd", "OK");
                return;
            }

            // Voorbeeld: vraag de gebruiker om een bericht in te geven
            string message = await Shell.Current.DisplayPromptAsync("Nieuwe notificatie", "Voer bericht in:");

            if (string.IsNullOrWhiteSpace(message))
                return;

            var notification = new Notification
            {
                PatientId = PatientId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _firebaseService.AddOrReplaceNotificationAsync(notification);

            // Push triggeren
            //await _firebaseService.SendPushNotificationAsync(notification);

            await LoadPatientAndDiaryItemsAsync();
        }
    }
}
