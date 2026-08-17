namespace YellowPad.Services;

public interface IVoiceCommandService
{
    bool IsListening { get; }
    event EventHandler<VoicePhraseRecognizedEventArgs>? PhraseRecognized;
    event EventHandler? ListeningChanged;
    VoiceCommandStartResult Start();
    void Stop();
}

public sealed class VoicePhraseRecognizedEventArgs(string phrase) : EventArgs
{
    public string Phrase { get; } = phrase;
}

public readonly record struct VoiceCommandStartResult(bool Started, string? ErrorMessage)
{
    public static VoiceCommandStartResult Success() => new(true, null);
    public static VoiceCommandStartResult Failure(string message) => new(false, message);
}
