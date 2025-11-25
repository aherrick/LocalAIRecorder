using LocalAIRecorder.Services;

namespace LocalAIRecorder;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Check for updates in background
        Task.Run(UpdateService.CheckForUpdatesAsync);
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        return new Window(new AppShell());
    }
}