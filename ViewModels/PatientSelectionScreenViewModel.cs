using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using zorgApp.Models;
using zorgApp.Services;

namespace zorgApp.ViewModels;

public partial class PatientSelectionScreenViewModel : ObservableObject
{
    private readonly FirebaseService _firebaseService; // Firebase connection

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    public ObservableCollection<Patient> Patients { get; } = new();

    public PatientSelectionScreenViewModel()
    {
        _firebaseService = new FirebaseService();

        // On ViewModel creation, start loading patients
        LoadPatientsCommand = new AsyncRelayCommand(LoadPatientsAsync);
        AddPatientCommand = new AsyncRelayCommand(AddPatientAsync);
        RemovePatientCommand = new AsyncRelayCommand<Patient>(RemovePatientAsync);

        // Optionally preload data:
        Task.Run(async () => await LoadPatientsAsync());
    }

    // Commands
    public IAsyncRelayCommand LoadPatientsCommand { get; }
    public IAsyncRelayCommand AddPatientCommand { get; }
    public IAsyncRelayCommand<Patient> RemovePatientCommand { get; }

    // -------------------------------
    // Load Patients
    // -------------------------------
    private async Task LoadPatientsAsync()
    {
        if (isBusy) return;

        try
        {
            isBusy = true;
            isRefreshing = true;

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
            isBusy = false;
            isRefreshing = false;
        }
    }

    // -------------------------------
    // Add Patient
    // -------------------------------
    private async Task AddPatientAsync()
    {
        try
        {
            // For now, create a basic test patient — later you could replace this
            // with a popup form to gather input from the user.
            var newPatient = new Patient
            {
                Name = $"Nieuwe Patiënt {Patients.Count + 1}",
                Email = $"patient{Patients.Count + 1}@zorg.nl"
            };

            // Save to Firebase
            var firebaseId = await _firebaseService.AddPatientAsync(newPatient);

            if (!string.IsNullOrEmpty(firebaseId))
            {
                newPatient.FirebaseId = firebaseId;
                Patients.Add(newPatient);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding patient: {ex.Message}");
        }
    }

    // -------------------------------
    // Remove Patient
    // -------------------------------
    private async Task RemovePatientAsync(Patient? patient)
    {
        if (patient == null) return;

        try
        {
            // Delete from Firebase first
            if (!string.IsNullOrEmpty(patient.FirebaseId))
            {
                await _firebaseService.DeletePatientAsync(patient.FirebaseId);
            }

            // Then remove locally from the UI list
            Patients.Remove(patient);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error removing patient: {ex.Message}");
        }
    }
}