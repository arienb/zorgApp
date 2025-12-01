using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using zorgApp.Models;
using zorgApp.Services;

namespace zorgApp.ViewModels;

public partial class NewPatientViewModel : ObservableObject
{
    private readonly FirebaseService _firebaseService;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string ageText = string.Empty;

    [ObservableProperty]
    private string roomNumber = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? patientId;

    [ObservableProperty]
    private bool isEditMode;

    public NewPatientViewModel(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public async Task LoadPatientAsync(string firebaseId)
    {
        try
        {
            IsLoading = true;
            var patients = await _firebaseService.GetPatientsAsync();
            var patient = patients.FirstOrDefault(p => p.FirebaseId == firebaseId);

            if (patient != null)
            {
                IsEditMode = true;
                PatientId = patient.FirebaseId;
                Name = patient.Name ?? string.Empty;
                Email = patient.Email ?? string.Empty;
                AgeText = patient.Age.ToString() ?? string.Empty;
                RoomNumber = patient.RoomNumber ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading patient: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kon patiënt niet laden: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SavePatientAsync()
    {
        // Validatie - alle velden zijn verplicht
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlert("Validatie", "Naam is verplicht", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            await Shell.Current.DisplayAlert("Validatie", "Email is verplicht", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(AgeText))
        {
            await Shell.Current.DisplayAlert("Validatie", "Leeftijd is verplicht", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(RoomNumber))
        {
            await Shell.Current.DisplayAlert("Validatie", "Kamernummer is verplicht", "OK");
            return;
        }

        // Valideer of leeftijd een geldig getal is
        if (!int.TryParse(AgeText, out int parsedAge) || parsedAge <= 0)
        {
            await Shell.Current.DisplayAlert("Validatie", "Voer een geldige leeftijd in", "OK");
            return;
        }

        IsLoading = true;

        try
        {
            if (IsEditMode && !string.IsNullOrEmpty(PatientId))
            {
                // Update existing patient
                var updatedPatient = new Patient
                {
                    FirebaseId = PatientId,
                    Name = Name,
                    Email = Email,
                    Age = parsedAge,
                    RoomNumber = RoomNumber
                };

                // Get the existing patient to preserve UniqueCode
                var patients = await _firebaseService.GetPatientsAsync();
                var existingPatient = patients.FirstOrDefault(p => p.FirebaseId == PatientId);
                if (existingPatient != null)
                {
                    updatedPatient.UniqueCode = existingPatient.UniqueCode;
                }

                await _firebaseService.UpdatePatientAsync(PatientId, updatedPatient);

                await Shell.Current.DisplayAlert(
                    "Bijgewerkt",
                    $"Patiënt {Name} is bijgewerkt.",
                    "OK"
                );
            }
            else
            {
                // Add new patient
                var newPatient = new Patient
                {
                    Name = Name,
                    Email = Email,
                    Age = parsedAge,
                    RoomNumber = RoomNumber
                };

                var firebaseId = await _firebaseService.AddPatientAsync(newPatient);

                if (!string.IsNullOrEmpty(firebaseId))
                {
                    newPatient.FirebaseId = firebaseId;

                    await Shell.Current.DisplayAlert(
                        "Patiënt Toegevoegd",
                        $"Patiënt {Name} is toegevoegd.\n\nUnieke code: {newPatient.UniqueCode}\n\nGeef deze code aan de patiënt voor toegang tot hun dagboek.",
                        "OK"
                    );
                }
            }

            // Navigate terug en refresh de patiëntenlijst
            MessagingCenter.Send(this, "PatientAdded");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving patient: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kon patiënt niet opslaan: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
