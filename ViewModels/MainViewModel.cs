using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalAIRecorder.Models;
using LocalAIRecorder.Services;

namespace LocalAIRecorder.ViewModels;

public partial class MainViewModel(
    AudioService audioService,
    DatabaseService databaseService,
    WhisperService whisperService
) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Recording> recordings = new();

    [ObservableProperty]
    private bool isRecording;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusText = "Ready";

    [ObservableProperty]
    private string recordButtonText = "Start Recording";

    [ObservableProperty]
    private Color recordButtonColor = Colors.Red;

    public async Task InitializeAsync()
    {
        await LoadRecordingsAsync();
    }

    [RelayCommand]
    private async Task LoadRecordingsAsync()
    {
        Recordings.Clear();
        var recordings = await databaseService.GetRecordingsAsync();
        foreach (var recording in recordings)
        {
            Recordings.Add(recording);
        }
    }

    [RelayCommand]
    private async Task ToggleRecordingAsync()
    {
        if (IsRecording)
        {
            await StopRecordingAsync();
        }
        else
        {
            await StartRecordingAsync();
        }
    }

    private async Task StartRecordingAsync()
    {
        // Check permissions first
        PermissionStatus status = PermissionStatus.Granted;
#if !WINDOWS
        status = await Permissions.RequestAsync<Permissions.Microphone>();
#endif
        if (status != PermissionStatus.Granted)
        {
            StatusText = "Microphone permission denied.";
            return;
        }

        IsBusy = true;
        await audioService.StartRecordingAsync();
        IsRecording = true;
        RecordButtonText = "Stop Recording";
        RecordButtonColor = Colors.DarkRed;
        StatusText = "Recording...";
        IsBusy = false;
    }

    private async Task StopRecordingAsync()
    {
        IsBusy = true;

        var (filePath, duration) = await audioService.StopRecordingAsync();
        IsRecording = false;
        RecordButtonText = "Start Recording";
        RecordButtonColor = Colors.Red;

        if (!string.IsNullOrEmpty(filePath))
        {
            var recording = new Recording
            {
                Filename = Path.GetFileName(filePath),
                FilePath = filePath,
                CreatedAt = DateTime.Now,
                Transcript = string.Empty,
                DurationSeconds = duration,
            };
            await databaseService.SaveRecordingAsync(recording);
            Recordings.Insert(0, recording);

            StatusText = "Recording saved.";

            // Navigate to DetailsPage
            var navigationParameter = new Dictionary<string, object>
            {
                { "RecordingId", recording.Id },
            };
            await Shell.Current.GoToAsync(nameof(DetailsPage), navigationParameter);
        }
        else
        {
            StatusText = "No audio captured.";
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task SelectRecordingAsync(Recording recording)
    {
        if (recording == null)
            return;

        var navigationParameter = new Dictionary<string, object>
        {
            { "RecordingId", recording.Id },
        };
        await Shell.Current.GoToAsync(nameof(DetailsPage), navigationParameter);
    }

    [RelayCommand]
    private async Task DownloadModelsAsync()
    {
        IsBusy = true;
        StatusText = "Ensuring Whisper model is available...";

        try
        {
            await whisperService.EnsureModelAsync();
            StatusText = "Whisper model ready.";
        }
        catch (Exception ex)
        {
            StatusText = "Error: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}