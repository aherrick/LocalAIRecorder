using System.Collections.ObjectModel;
using LocalAIRecorder.Services;

namespace LocalAIRecorder;

public partial class MainPage : ContentPage
{
    private readonly AudioService _audioService;
    public ObservableCollection<string> Recordings { get; } = new();

    public MainPage(AudioService audioService)
    {
        InitializeComponent();
        _audioService = audioService;
        RecordingsCollectionView.ItemsSource = Recordings;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadRecordings();
    }

    private async void OnDownloadModelsClicked(object sender, EventArgs e)
    {
        DownloadModelsButton.IsEnabled = false;
        DownloadProgress.IsVisible = true;

        try
        {
            StatusLabel.Text = "Ensuring Whisper model is available...";
            await WhisperModelProvider.EnsureModelAsync();
            StatusLabel.Text = "Whisper model ready.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Error: " + ex.Message;
        }
        finally
        {
            DownloadModelsButton.IsEnabled = true;
            DownloadProgress.IsVisible = false;
        }
    }

    private async void OnRecordClicked(object sender, EventArgs e)
    {
        try
        {
            if (_audioService.IsRecording)
            {
                RecordingIndicator.IsVisible = true;
                RecordingIndicator.IsRunning = true;

                var filePath = await _audioService.StopRecordingAsync();
                RecordButton.Text = "Start Recording";
                RecordButton.BackgroundColor = Colors.Red;
                if (!string.IsNullOrEmpty(filePath))
                {
                    Recordings.Add(filePath);
                    StatusLabel.Text = "Recording saved.";
                }
                else
                {
                    StatusLabel.Text = "No audio captured.";
                }

                RecordingIndicator.IsRunning = false;
                RecordingIndicator.IsVisible = false;
            }
            else
            {
                // Check permissions first
                PermissionStatus status = PermissionStatus.Granted;
#if !WINDOWS
                status = await Permissions.RequestAsync<Permissions.Microphone>();
#endif
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlertAsync(
                        "Permission",
                        "Microphone permission is required.",
                        "OK"
                    );
                    return;
                }

                RecordingIndicator.IsVisible = true;
                RecordingIndicator.IsRunning = true;
                await _audioService.StartRecordingAsync();
                RecordButton.Text = "Stop Recording";
                RecordButton.BackgroundColor = Colors.DarkRed;
                StatusLabel.Text = "Recording...";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
            RecordingIndicator.IsRunning = false;
            RecordingIndicator.IsVisible = false;
        }
    }

    private async void OnRecordingSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is string filePath)
        {
            // Navigate to DetailsPage
            var navigationParameter = new Dictionary<string, object> { { "FilePath", filePath } };
            await Shell.Current.GoToAsync(nameof(DetailsPage), navigationParameter);

            // Deselect
            RecordingsCollectionView.SelectedItem = null;
        }
    }

    private static void LoadRecordings()
    {
        // Placeholder for loading existing recordings
    }
}