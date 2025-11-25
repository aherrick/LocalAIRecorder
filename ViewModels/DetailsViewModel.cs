using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalAIRecorder.Models;
using LocalAIRecorder.Services;

namespace LocalAIRecorder.ViewModels;

// ViewModel for the details page. MAUI Shell passes navigation query parameters
// via IQueryAttributable.ApplyQueryAttributes; we use RecordingId to load data.
public partial class DetailsViewModel(
    WhisperService whisperService,
    ILocalIntelligenceService localIntelligence,
    DatabaseService databaseService
) : ObservableObject, IQueryAttributable
{
    private int _recordingId;
    private Recording _currentRecording;

    [ObservableProperty]
    public partial string Transcript { get; set; }

    [ObservableProperty]
    public partial bool IsTranscribing { get; set; }

    [ObservableProperty]
    public partial string ChatInput { get; set; }

    [ObservableProperty]
    public partial bool IsThinking { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ChatMessage> ChatMessages { get; set; } = [];

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (
            query.TryGetValue("RecordingId", out var recordingIdObj)
            && recordingIdObj is int recordingId
        )
        {
            _recordingId = recordingId;
            await LoadRecordingAsync();
        }
    }

    private async Task LoadRecordingAsync()
    {
        _currentRecording = await databaseService.GetRecordingAsync(_recordingId);
        if (_currentRecording == null)
            return;

        // Load existing transcript if available
        if (!string.IsNullOrEmpty(_currentRecording.Transcript))
        {
            Transcript = _currentRecording.Transcript;
        }
        else
        {
            // Auto-transcribe if not already done
            await TranscribeAsync();
        }

        // Load chat history
        var messages = await databaseService.GetChatMessagesAsync(_recordingId);
        ChatMessages.Clear();
        foreach (var msg in messages)
        {
            ChatMessages.Add(msg);
        }
    }

    [RelayCommand]
    private async Task TranscribeAsync()
    {
        if (_currentRecording == null || string.IsNullOrEmpty(_currentRecording.FilePath))
            return;

        IsTranscribing = true;

        try
        {
            var transcript = await whisperService.TranscribeWavFileAsync(
                _currentRecording.FilePath,
                "en"
            );
            Transcript = transcript;

            // Save transcript to database
            _currentRecording.Transcript = transcript;
            await databaseService.SaveRecordingAsync(_currentRecording);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            IsTranscribing = false;
        }
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(ChatInput) || _currentRecording == null)
            return;

        var text = ChatInput;
        ChatInput = string.Empty;

        // Save and display user message
        var userMessage = new ChatMessage
        {
            RecordingId = _recordingId,
            Text = text,
            IsUser = true,
            CreatedAt = DateTime.Now,
        };
        await databaseService.SaveChatMessageAsync(userMessage);
        ChatMessages.Add(userMessage);

        // Show thinking indicator
        IsThinking = true;

        // Build prompt with context for first message
        var prompt = text;
        if (ChatMessages.Count == 1 && !string.IsNullOrEmpty(Transcript))
        {
            prompt = $"Context: {Transcript}\n\nUser: {text}";
        }

        // Get AI response
        var answer = await localIntelligence.AskAsync(prompt);

        // Hide thinking indicator
        IsThinking = false;

        // Save and display AI response
        var responseMessage = new ChatMessage
        {
            RecordingId = _recordingId,
            Text = answer,
            IsUser = false,
            CreatedAt = DateTime.Now,
        };
        await databaseService.SaveChatMessageAsync(responseMessage);
        ChatMessages.Add(responseMessage);
    }
}