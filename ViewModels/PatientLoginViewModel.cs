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

public partial class PatientLoginViewModel : ObservableObject
{
    private readonly FirebaseService _firebaseService;

    [ObservableProperty]
    private string uniqueCode = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public PatientLoginViewModel(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(UniqueCode))
        {
            await Shell.Current.DisplayAlert("Fout", "Voer een unieke code in", "OK");
            return;
        }

        IsLoading = true;

        try
        {
            var patient = await _firebaseService.GetPatientByUniqueCodeAsync(UniqueCode);

            if (patient != null)
            {
                // Navigate to diary page with patient ID
                await Shell.Current.GoToAsync($"DiaryPage?PatientId={patient.FirebaseId}");
            }
            else
            {
                await Shell.Current.DisplayAlert("Fout", "Ongeldige code. Probeer opnieuw.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Fout", $"Er is een fout opgetreden: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
