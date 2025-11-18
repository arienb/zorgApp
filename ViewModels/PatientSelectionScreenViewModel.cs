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

        Task.Run(async () => await LoadPatientsAsync());
    }

    public IAsyncRelayCommand LoadPatientsCommand { get; }
    public IAsyncRelayCommand AddPatientCommand { get; }
    public IAsyncRelayCommand<Patient> RemovePatientCommand { get; }
    public IAsyncRelayCommand<Patient> SelectPatientCommand { get; }

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
            string name = await Shell.Current.DisplayPromptAsync(
                "Nieuwe Patiënt",
                "Naam:"
            );

            if (string.IsNullOrWhiteSpace(name))
                return;

            string email = await Shell.Current.DisplayPromptAsync(
                "Nieuwe Patiënt",
                "Email:"
            );

            string ageStr = await Shell.Current.DisplayPromptAsync(
                "Nieuwe Patiënt",
                "Leeftijd:",
                keyboard: Keyboard.Numeric
            );

            int? age = null;
            if (int.TryParse(ageStr, out int parsedAge))
                age = parsedAge;

            string roomNumber = await Shell.Current.DisplayPromptAsync(
                "Nieuwe Patiënt",
                "Kamernummer:"
            );

            var newPatient = new Patient
            {
                Name = name,
                Email = email,
                Age = age,
                RoomNumber = roomNumber
            };

            var firebaseId = await _firebaseService.AddPatientAsync(newPatient);

            if (!string.IsNullOrEmpty(firebaseId))
            {
                newPatient.FirebaseId = firebaseId;
                Patients.Add(newPatient);

                await Shell.Current.DisplayAlert(
                    "Patiënt Toegevoegd",
                    $"Patiënt {name} is toegevoegd.\n\nUnieke code: {newPatient.UniqueCode}\n\nGeef deze code aan de patiënt voor toegang tot hun dagboek.",
                    "OK"
                );
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding patient: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kon patiënt niet toevoegen: {ex.Message}", "OK");
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
}