using zorgApp.Views;

namespace zorgApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(AddDiaryItemPage), typeof(AddDiaryItemPage));
            Routing.RegisterRoute(nameof(DiaryItemDetailsPage), typeof(DiaryItemDetailsPage));
            Routing.RegisterRoute(nameof(DiaryItemsPage), typeof(DiaryItemsPage));
            Routing.RegisterRoute(nameof(PatientSelectionPage), typeof(PatientSelectionPage));
            Routing.RegisterRoute(nameof(PatientLoginPage), typeof(PatientLoginPage));
            Routing.RegisterRoute(nameof(NurseLoginPage), typeof(NurseLoginPage));
            Routing.RegisterRoute(nameof(NewPatientPage), typeof(NewPatientPage));
            Routing.RegisterRoute(nameof(PatientProfilePage), typeof(PatientProfilePage));
        }
    }
}
