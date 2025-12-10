using Microsoft.Extensions.Logging;
using zorgApp.Services;
using zorgApp.ViewModels;
using zorgApp.Views;
using CommunityToolkit.Maui;

namespace zorgApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Services
            builder.Services.AddSingleton<FirebaseService>();

            // Register ViewModels
            builder.Services.AddTransient<DiaryPageViewModel>();
            builder.Services.AddTransient<AddDiaryItemViewModel>();
            builder.Services.AddTransient<DiaryPageDetailsViewModel>();
            builder.Services.AddTransient<StartupSelectionViewModel>();
            builder.Services.AddTransient<PatientSelectionScreenViewModel>();
            builder.Services.AddTransient<PatientLoginViewModel>();
            builder.Services.AddTransient<NurseLoginViewModel>();
            builder.Services.AddTransient<NewPatientViewModel>();
            builder.Services.AddTransient<PatientProfileViewModel>();

            // Register Views
            builder.Services.AddTransient<DiaryItemsPage>();
            builder.Services.AddTransient<AddDiaryItemPage>();
            builder.Services.AddTransient<DiaryItemDetailsPage>();
            builder.Services.AddTransient<StartupSelectionPage>();
            builder.Services.AddTransient<PatientSelectionPage>();
            builder.Services.AddTransient<PatientLoginPage>();
            builder.Services.AddTransient<NurseLoginPage>();
            builder.Services.AddTransient<NewPatientPage>();
            builder.Services.AddTransient<PatientProfilePage>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
