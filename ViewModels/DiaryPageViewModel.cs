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
        private bool isRefreshing;

        [ObservableProperty]
        private bool isNurseMode;

        [ObservableProperty]
        private bool hasUnreadNotifications;

        [ObservableProperty]
        private int unreadNotificationCount;

        [ObservableProperty]
        private bool hasNotifications;

        public ObservableCollection<DiaryItem> DiaryItems { get; set; }
        public ObservableCollection<Notification> Notifications { get; set; }

        public DiaryPageViewModel(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
            DiaryItems = new ObservableCollection<DiaryItem>();
            Notifications = new ObservableCollection<Notification>();
            
            // Check if logged in as nurse (has CurrentDepartment preference)
            IsNurseMode = !string.IsNullOrEmpty(Preferences.Get("CurrentDepartment", string.Empty));
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
            await LoadNotificationsAsync();
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
        private async Task LoadNotificationsAsync()
        {
            if (string.IsNullOrEmpty(PatientId))
                return;

            try
            {
                var notifications = await _firebaseService.GetNotificationsAsync(PatientId);
                
                Notifications.Clear();
                foreach (var notification in notifications)
                {
                    Notifications.Add(notification);
                }

                // Update notification properties
                HasNotifications = Notifications.Count > 0;
                UnreadNotificationCount = Notifications.Count(n => !n.IsRead);
                HasUnreadNotifications = UnreadNotificationCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading notifications: {ex.Message}");
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

            await Shell.Current.GoToAsync($"AddDiaryItemPage?PatientId={PatientId}");
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
            
            await Shell.Current.GoToAsync($"DiaryItemDetailsPage?PatientId={PatientId}&ItemId={item.Id}");
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

            await Shell.Current.GoToAsync("PatientProfilePage", new Dictionary<string, object>
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
        private async Task AddNotificationAsync()
        {
            if (string.IsNullOrEmpty(PatientId))
            {
                await Shell.Current.DisplayAlert("Fout", "Geen patiënt geselecteerd", "OK");
                return;
            }

            string message = await Shell.Current.DisplayPromptAsync("Nieuwe notificatie", "Voer bericht in:");

            if (string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                var notification = new Notification
                {
                    PatientId = PatientId,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    IsRead = false
                };

                await _firebaseService.AddNotificationAsync(notification);
                await LoadNotificationsAsync();
                
                await Shell.Current.DisplayAlert("Succes", "Notificatie toegevoegd!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fout", $"Kon notificatie niet toevoegen: {ex.Message}", "OK");
            }
        }

        //------- voor patiënt/familie -------//
        [RelayCommand]
        private async Task MarkNotificationAsReadAsync(Notification notification)
        {
            if (notification == null || string.IsNullOrEmpty(PatientId))
                return;

            try
            {
                await _firebaseService.MarkNotificationAsReadAsync(PatientId, notification.Id);
                await LoadNotificationsAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fout", $"Kon notificatie niet markeren als gelezen: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task DeleteNotificationAsync(Notification notification)
        {
            if (notification == null || string.IsNullOrEmpty(PatientId))
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Verwijderen",
                "Weet je zeker dat je deze notificatie wilt verwijderen?",
                "Ja",
                "Nee"
            );

            if (!confirm)
                return;

            try
            {
                await _firebaseService.DeleteNotificationAsync(PatientId, notification.Id);
                await LoadNotificationsAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fout", $"Kon notificatie niet verwijderen: {ex.Message}", "OK");
            }
        }
    }
}
