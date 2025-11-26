using Plugin.Maui.Audio;
using Plugin.Maui.Audio.AudioListeners;

namespace LocalAIRecorder.Services;

public class AudioService
{
    private IAudioStreamer _streamer;
    private MemoryStream _pcmBuffer;
    private string _targetPath;
    private DateTime _recordingStartTime;

    public async Task StartRecordingAsync()
    {
        _streamer = AudioManager.Current.CreateStreamer();
        _streamer.Options.Channels = ChannelType.Mono;
        _streamer.Options.BitDepth = BitDepth.Pcm16bit;
        _streamer.Options.SampleRate = 16000;

        _pcmBuffer = new MemoryStream();

        // Generate unique timestamped filename and save to AppDataDirectory for persistence
        var fileName = $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
        _targetPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        _streamer.OnAudioCaptured += OnAudioCaptured;
        _recordingStartTime = DateTime.Now;
        await _streamer.StartAsync();
    }

    public async Task<(string filePath, double durationSeconds)> StopRecordingAsync()
    {
        if (_streamer == null || _pcmBuffer == null || _targetPath == null)
            return (string.Empty, 0);

        await _streamer.StopAsync();
        _streamer.OnAudioCaptured -= OnAudioCaptured;
        _streamer = null;

        // Write a proper WAV file with RIFF header for 16kHz mono PCM
        var pcmData = _pcmBuffer.ToArray();
        await _pcmBuffer.DisposeAsync();
        _pcmBuffer = null;

        var header = PcmAudioHelpers.CreateWavFileHeader(pcmData.Length, 16000, 1, 16);

        await using var fileStream = File.Create(_targetPath);
        await fileStream.WriteAsync(header);
        await fileStream.WriteAsync(pcmData);

        var result = _targetPath;
        var duration = (DateTime.Now - _recordingStartTime).TotalSeconds;
        _targetPath = null;
        return (result, duration);
    }

    private void OnAudioCaptured(object _, AudioStreamEventArgs e)
    {
        if (_pcmBuffer == null)
            return;

        // Accumulate raw PCM into memory buffer
        _pcmBuffer.Write(e.Audio, 0, e.Audio.Length);
    }
}