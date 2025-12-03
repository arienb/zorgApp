using zorgApp.Views;

namespace zorgApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(AddDiaryItemView), typeof(AddDiaryItemView));
            Routing.RegisterRoute(nameof(DiaryPageDetailsView), typeof(DiaryPageDetailsView));
            Routing.RegisterRoute(nameof(DiaryPage), typeof(DiaryPage));
            Routing.RegisterRoute(nameof(PatientSelectionScreen), typeof(PatientSelectionScreen));
            Routing.RegisterRoute(nameof(PatientLoginPage), typeof(PatientLoginPage));
            Routing.RegisterRoute(nameof(NewPatientView), typeof(NewPatientView));
            Routing.RegisterRoute(nameof(PatientProfilePage), typeof(PatientProfilePage));
        }
    }
}
