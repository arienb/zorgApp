using zorgApp.ViewModels;

namespace zorgApp.Views;

[QueryProperty(nameof(PatientId), nameof(PatientId))]
public partial class NewPatientView : ContentPage
{
    private readonly NewPatientViewModel _viewModel;
    private string? _patientId;

    public string? PatientId
    {
        get => _patientId;
        set
        {
            _patientId = value;
            if (!string.IsNullOrEmpty(value) && _viewModel != null)
            {
                Task.Run(async () => await _viewModel.LoadPatientAsync(value));
            }
        }
    }

    public NewPatientView(NewPatientViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}