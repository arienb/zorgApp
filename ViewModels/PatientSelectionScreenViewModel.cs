using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using zorgApp.Models;
using zorgApp.Services;

namespace zorgApp.ViewModels;

public partial class PatientSelectionScreenViewModel : ObservableObject
{
    private readonly FirebaseService _firebaseService;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    public ObservableCollection<Patient> Patients { get; } = new();

    public PatientSelectionScreenViewModel(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;

        LoadPatientsCommand = new AsyncRelayCommand(LoadPatientsAsync);
        AddPatientCommand = new AsyncRelayCommand(AddPatientAsync);
        RemovePatientCommand = new AsyncRelayCommand<Patient>(RemovePatientAsync);
        SelectPatientCommand = new AsyncRelayCommand<Patient>(SelectPatientAsync);
        EditPatientCommand = new AsyncRelayCommand<Patient>(EditPatientAsync);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);

        // Subscribe to PatientAdded message
        MessagingCenter.Subscribe<NewPatientViewModel>(this, "PatientAdded", async (sender) =>
        {
            await LoadPatientsAsync();
        });

        Task.Run(async () => await LoadPatientsAsync());
    }

    public IAsyncRelayCommand LoadPatientsCommand { get; }
    public IAsyncRelayCommand AddPatientCommand { get; }
    public IAsyncRelayCommand<Patient> RemovePatientCommand { get; }
    public IAsyncRelayCommand<Patient> SelectPatientCommand { get; }
    public IAsyncRelayCommand<Patient> EditPatientCommand { get; }
    public IAsyncRelayCommand LogoutCommand { get; }

    private async Task LoadPatientsAsync()
    {
        if (IsBusy) return; 

        try
        {
            IsBusy = true;
            IsRefreshing = true;

            var patientsFromFirebase = await _firebaseService.GetPatientsAsync();
            System.Diagnostics.Debug.WriteLine($"Loaded {patientsFromFirebase.Count} patients from Firebase");

            Patients.Clear();
            foreach (var p in patientsFromFirebase)
                Patients.Add(p);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading patients: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    private async Task AddPatientAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("NewPatientView");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to NewPatientView: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kon niet navigeren naar nieuwe patiënt pagina: {ex.Message}", "OK");
        }
    }

    private async Task EditPatientAsync(Patient? patient)
    {
        if (patient == null || string.IsNullOrEmpty(patient.FirebaseId))
            return;

        try
        {
            await Shell.Current.GoToAsync($"NewPatientView?PatientId={patient.FirebaseId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to edit patient: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kon niet navigeren naar bewerk pagina: {ex.Message}", "OK");
        }
    }

    private async Task RemovePatientAsync(Patient? patient)
    {
        if (patient == null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Verwijderen",
            $"Weet je zeker dat je {patient.Name} wilt verwijderen? Dit verwijdert ook alle dagboekitems van deze patiënt.",
            "Ja",
            "Nee"
        );

        if (!confirm) return;

        try
        {
            if (!string.IsNullOrEmpty(patient.FirebaseId))
            {
                await _firebaseService.DeletePatientAsync(patient.FirebaseId);
            }

            Patients.Remove(patient);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error removing patient: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kon patiënt niet verwijderen: {ex.Message}", "OK");
        }
    }

    private async Task SelectPatientAsync(Patient? patient)
    {
        if (patient == null) return;

        await Shell.Current.GoToAsync($"DiaryPage?PatientId={patient.FirebaseId}");
    }

    private async Task LogoutAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Uitloggen",
            "Weet je zeker dat je wilt uitloggen?",
            "Ja",
            "Nee"
        );

        if (!confirm) return;

        try
        {
            await Shell.Current.GoToAsync("//StartupSelectionPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error logging out: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kon niet uitloggen: {ex.Message}", "OK");
        }
    }
}