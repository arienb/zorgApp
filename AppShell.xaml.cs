using zorgApp.Views;

namespace zorgApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registreer routes voor navigation
            Routing.RegisterRoute(nameof(AddDiaryItemView), typeof(AddDiaryItemView));
            Routing.RegisterRoute(nameof(DiaryPageDetailsView), typeof(DiaryPageDetailsView));
        }
    }
}
