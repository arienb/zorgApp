using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

    [ObservableProperty]
    private string currentDepartment = string.Empty;

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

        // Gebruik WeakReferenceMessenger in plaats van MessagingCenter
        WeakReferenceMessenger.Default.Register<PatientAddedMessage>(this, async (recipient, message) =>
        {
            await LoadPatientsAsync();
        });

        // Get current department from preferences
        CurrentDepartment = Preferences.Get("CurrentDepartment", string.Empty);

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

            // Get current department from preferences
            CurrentDepartment = Preferences.Get("CurrentDepartment", string.Empty);

            if (string.IsNullOrEmpty(CurrentDepartment))
            {
                System.Diagnostics.Debug.WriteLine("No department set - loading all patients");
                var allPatients = await _firebaseService.GetPatientsAsync();
                
                Patients.Clear();
                foreach (var p in allPatients)
                    Patients.Add(p);
                    
                System.Diagnostics.Debug.WriteLine($"Loaded {allPatients.Count} patients (no department filter)");
            }
            else
            {
                // Load only patients from current department
                var patientsFromFirebase = await _firebaseService.GetPatientsByDepartmentAsync(CurrentDepartment);
                System.Diagnostics.Debug.WriteLine($"Loaded {patientsFromFirebase.Count} patients from department: {CurrentDepartment}");

                Patients.Clear();
                foreach (var p in patientsFromFirebase)
                    Patients.Add(p);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading patients: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kon patiënten niet laden: {ex.Message}", "OK");
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
            await Shell.Current.GoToAsync("NewPatientPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to NewPatientPage: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kon niet navigeren naar nieuwe patiënt pagina: {ex.Message}", "OK");
        }
    }

    private async Task EditPatientAsync(Patient? patient)
    {
        if (patient == null || string.IsNullOrEmpty(patient.FirebaseId))
            return;

        try
        {
            await Shell.Current.GoToAsync($"NewPatientPage?PatientId={patient.FirebaseId}");
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

        await Shell.Current.GoToAsync($"DiaryItemsPage?PatientId={patient.FirebaseId}");
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
            // Clear department preference
            Preferences.Remove("CurrentDepartment");
            
            await Shell.Current.GoToAsync("//StartupSelectionPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error logging out: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kon niet uitloggen: {ex.Message}", "OK");
        }
    }
}