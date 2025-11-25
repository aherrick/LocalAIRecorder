using SQLite;

namespace LocalAIRecorder.Models;

public class ChatMessage
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int RecordingId { get; set; }

    public string Text { get; set; }

    public bool IsUser { get; set; }

    public DateTime CreatedAt { get; set; }
}