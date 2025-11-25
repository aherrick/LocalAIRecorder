using Plugin.Maui.Audio;
using Plugin.Maui.Audio.AudioListeners;

namespace LocalAIRecorder.Services;

public class AudioService
{
    private IAudioStreamer _streamer;
    private MemoryStream _pcmBuffer;
    private string _targetPath;

    public AudioService()
    {
    }

    public bool IsRecording => _streamer != null && _streamer.IsStreaming;

    public async Task StartRecordingAsync()
    {
        if (IsRecording)
            return;

        _streamer = AudioManager.Current.CreateStreamer();
        _streamer.Options.Channels = ChannelType.Mono;
        _streamer.Options.BitDepth = BitDepth.Pcm16bit;
        _streamer.Options.SampleRate = 16000;

        _pcmBuffer = new MemoryStream();
        _targetPath = Path.Combine(FileSystem.CacheDirectory, "recording.wav");

        _streamer.OnAudioCaptured += OnAudioCaptured;
        await _streamer.StartAsync();
    }

    public async Task<string> StopRecordingAsync()
    {
        if (_streamer == null || _pcmBuffer == null || _targetPath == null)
            return string.Empty;

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
        _targetPath = null;
        return result;
    }

    private void OnAudioCaptured(object sender, AudioStreamEventArgs e)
    {
        if (_pcmBuffer == null)
            return;

        // Accumulate raw PCM into memory buffer
        _pcmBuffer.Write(e.Audio, 0, e.Audio.Length);
    }
}