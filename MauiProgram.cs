using LocalAIRecorder.Services;
using Microsoft.Extensions.Logging;

namespace LocalAIRecorder;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Services
        builder.Services.AddSingleton<AudioService>();
        builder.Services.AddSingleton<WhisperService>();

#if IOS || MACCATALYST
        builder.Services.AddSingleton<ILocalIntelligenceService, AppleLocalIntelligenceService>();
#elif WINDOWS
        builder.Services.AddSingleton<ILocalIntelligenceService, WindowsLocalIntelligenceService>();
#endif

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<DetailsPage>();

        return builder.Build();
    }
}