using SQLite;
using LocalAIRecorder.Models;

namespace LocalAIRecorder.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection _database;

    public async Task InitAsync()
    {
        if (_database != null)
            return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "localairecorder.db3");
        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<Recording>();
        await _database.CreateTableAsync<ChatMessage>();
    }

    public async Task<List<Recording>> GetRecordingsAsync()
    {
        await InitAsync();
        return await _database.Table<Recording>()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Recording> GetRecordingAsync(int id)
    {
        await InitAsync();
        return await _database.Table<Recording>()
            .Where(r => r.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveRecordingAsync(Recording recording)
    {
        await InitAsync();
        if (recording.Id != 0)
            return await _database.UpdateAsync(recording);
        else
            return await _database.InsertAsync(recording);
    }

    public async Task<int> DeleteRecordingAsync(Recording recording)
    {
        await InitAsync();
        
        // Delete associated chat messages
        await _database.Table<ChatMessage>()
            .Where(c => c.RecordingId == recording.Id)
            .DeleteAsync();
        
        // Delete the audio file
        if (File.Exists(recording.FilePath))
            File.Delete(recording.FilePath);
        
        return await _database.DeleteAsync(recording);
    }

    public async Task<List<ChatMessage>> GetChatMessagesAsync(int recordingId)
    {
        await InitAsync();
        return await _database.Table<ChatMessage>()
            .Where(c => c.RecordingId == recordingId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> SaveChatMessageAsync(ChatMessage message)
    {
        await InitAsync();
        
        if (message.Id != 0)
            return await _database.UpdateAsync(message);
        else
            return await _database.InsertAsync(message);
    }
}
