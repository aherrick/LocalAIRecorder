using LocalAIRecorder.Services;
using LocalAIRecorder.ViewModels;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace LocalAIRecorder;

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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<AudioService>();
        builder.Services.AddSingleton<WhisperService>();

#if IOS || MACCATALYST
        builder.Services.AddSingleton<ILocalIntelligenceService, AppleLocalIntelligenceService>();
#elif WINDOWS
        builder.Services.AddSingleton<ILocalIntelligenceService, WindowsLocalIntelligenceService>();
#elif ANDROID
        builder.Services.AddSingleton<ILocalIntelligenceService, AndroidLocalIntelligenceService>();
#endif

        // ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<DetailsViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<DetailsPage>();

        return builder.Build();
    }
}