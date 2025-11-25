using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalAIRecorder.Models;
using LocalAIRecorder.Services;

namespace LocalAIRecorder.ViewModels;

public partial class DetailsViewModel : ObservableObject, IQueryAttributable
{
    private readonly WhisperService _whisperService;
    private readonly ILocalIntelligenceService _localIntelligence;
    private readonly DatabaseService _databaseService;
    private int _recordingId;
    private Recording _currentRecording;

    [ObservableProperty]
    private string transcript = string.Empty;

    [ObservableProperty]
    private bool isTranscribing;

    [ObservableProperty]
    private string chatInput = string.Empty;

    [ObservableProperty]
    private bool isThinking;

    [ObservableProperty]
    private ObservableCollection<ChatMessage> chatMessages = new();

    public DetailsViewModel(WhisperService whisperService, ILocalIntelligenceService localIntelligence, DatabaseService databaseService)
    {
        _whisperService = whisperService;
        _localIntelligence = localIntelligence;
        _databaseService = databaseService;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("RecordingId"))
        {
            _recordingId = (int)query["RecordingId"];
            await LoadRecordingAsync();
        }
    }

    private async Task LoadRecordingAsync()
    {
        _currentRecording = await _databaseService.GetRecordingAsync(_recordingId);
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
        var messages = await _databaseService.GetChatMessagesAsync(_recordingId);
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
            var transcript = await _whisperService.TranscribeWavFileAsync(_currentRecording.FilePath, "en");
            Transcript = transcript;

            // Save transcript to database
            _currentRecording.Transcript = transcript;
            await _databaseService.SaveRecordingAsync(_currentRecording);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
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
            CreatedAt = DateTime.Now
        };
        await _databaseService.SaveChatMessageAsync(userMessage);
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
        var answer = await _localIntelligence.AskAsync(prompt);

        // Hide thinking indicator
        IsThinking = false;

        // Save and display AI response
        var responseMessage = new ChatMessage
        {
            RecordingId = _recordingId,
            Text = answer,
            IsUser = false,
            CreatedAt = DateTime.Now
        };
        await _databaseService.SaveChatMessageAsync(responseMessage);
        ChatMessages.Add(responseMessage);
    }
}
