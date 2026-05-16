using Microsoft.Extensions.Logging;
using StudyFlow.Service;
using StudyFlow.Service.ClassroomService;
using StudyFlow.Service.DBService;
using StudyFlow.Service.DBService.Firebase;
using StudyFlow.Service.DBService.FireBase;
using StudyFlow.ViewModels;
using StudyFlow.Views;

namespace StudyFlow
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    // Register the default fonts
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                    // Register Material Icons font so we can use icons as text
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            // Register all Views, ViewModels and Services for Dependency Injection
            builder.RegisterViews()
                   .RegisterViewModels()
                   .RegisterServices();

           

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        // Register all Views so they can be injected
        public static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
        {
            // Transient means a new instance is created every time it is requested
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<SignInView>();
            builder.Services.AddTransient<SignUpView>();
            builder.Services.AddTransient<MainPageView>();
            builder.Services.AddTransient<AccountView>();
            builder.Services.AddTransient<AdminView>();
            builder.Services.AddTransient<UsersListView>();
            builder.Services.AddTransient<AssignmentDetailView>();
            builder.Services.AddTransient<AddAssignmentView>();
            builder.Services.AddTransient<UserProfileView>();
            return builder;
        }

        // Register all ViewModels so they can be injected into Views
        public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
        {
            // Transient means a new instance is created every time it is requested
            builder.Services.AddTransient<AppShellViewModel>();
            builder.Services.AddTransient<SignInViewModel>();
            builder.Services.AddTransient<SignUpViewModel>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<AccountViewModel>();
            builder.Services.AddTransient<AdminViewModel>();
            builder.Services.AddTransient<UsersListViewModel>();
            builder.Services.AddTransient<AssignmentDetailViewModel>();
            builder.Services.AddTransient<AddAssignmentViewModel>();
            builder.Services.AddTransient<UserProfileViewModel>();
            return builder;
        }

        // Register all Services so they can be injected wherever needed
        public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
        {
            // Singleton means only ONE instance exists for the whole app lifetime
            builder.Services.AddSingleton<IAppLogger, LogService>();
            builder.Services.AddSingleton<IAlertService, AlertService>();
            builder.Services.AddSingleton<IAuthService, FireBaseAuthService>();

            // Transient because each repository operation is independent
            builder.Services.AddTransient<IAppUserRepository, FireBaseUserRepository>();
            // Register classroom service - swap to GoogleClassroomService later
            builder.Services.AddSingleton<IClassRooomService, GoogleClassroomService>();
            // Register subtask generator service
            builder.Services.AddSingleton<SubtaskGeneratorService>();
            // Register progress repository
            builder.Services.AddSingleton<IProgressRepository, FireBaseProgressRepository>();
#if ANDROID
            builder.Services.AddSingleton<IGoogleAuthService, StudyFlow.GoogleAuthService>();
#endif
            return builder;
        }
    }
}