using SQLite;

namespace LocalAIRecorder.Models;

public class Recording
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Filename { get; set; }

    public string FilePath { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Transcript { get; set; }

    public double DurationSeconds { get; set; }
}