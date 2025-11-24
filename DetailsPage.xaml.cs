using LocalAIRecorder.Services;
using System.Collections.ObjectModel;

namespace LocalAIRecorder;

public partial class DetailsPage : ContentPage, IQueryAttributable
{
    private readonly WhisperService _whisperService;
    private readonly ILocalIntelligenceService _localIntelligence;
    private string? _audioFilePath;

    public ObservableCollection<ChatMessage> ChatMessages { get; set; } = new();

    public DetailsPage(WhisperService whisperService, ILocalIntelligenceService localIntelligence)
    {
        InitializeComponent();
        _whisperService = whisperService;
        _localIntelligence = localIntelligence;
        ChatCollectionView.ItemsSource = ChatMessages;
        
        // Simple converter for demo (should be in resources)
        Resources.Add("BoolToColorConverter", new BoolToColorConverter());
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("FilePath"))
        {
            _audioFilePath = query["FilePath"] as string;
        }
    }

    private async void OnTranscribeClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_audioFilePath)) return;

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        TranscribeButton.IsEnabled = false;

        try
        {
            var transcript = await _whisperService.TranscribeWavFileAsync(_audioFilePath, "en");
            TranscriptEditor.Text = transcript;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            TranscribeButton.IsEnabled = true;
        }
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        var text = ChatInput.Text;
        if (string.IsNullOrWhiteSpace(text)) return;

        ChatMessages.Add(new ChatMessage { Text = text, IsUser = true });
        ChatInput.Text = "";

        var responseMessage = new ChatMessage { Text = "", IsUser = false, IsThinking = true };
        ChatMessages.Add(responseMessage);

        // Include transcript in context if it's the first message or manage context
        var prompt = text;
        if (ChatMessages.Count == 2 && !string.IsNullOrEmpty(TranscriptEditor.Text))
        {
            prompt = $"Context: {TranscriptEditor.Text}\n\nUser: {text}";
        }

        var answer = await _localIntelligence.AskAsync(prompt);
        responseMessage.IsThinking = false;
        responseMessage.Text = answer;
    }
}

public class ChatMessage : System.ComponentModel.INotifyPropertyChanged
{
    private string _text = string.Empty;
    public string Text 
    { 
        get => _text; 
        set 
        { 
            _text = value; 
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Text))); 
        } 
    }
    public bool IsUser { get; set; }

    private bool _isThinking;
    public bool IsThinking
    {
        get => _isThinking;
        set
        {
            _isThinking = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsThinking)));
        }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? Colors.LightBlue : Colors.LightGray;
        }
        return Colors.LightGray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}