using System.Globalization;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;

namespace YellowPad.Services;

public sealed class WindowsVoiceCommandService(ILogger<WindowsVoiceCommandService> logger)
    : IVoiceCommandService, IDisposable
{
    private readonly object _syncRoot = new();
    private SpeechRecognitionEngine? _recognizer;
    private bool _isListening;
    private bool _isDisposed;

    public event EventHandler<VoicePhraseRecognizedEventArgs>? PhraseRecognized;
    public event EventHandler? ListeningChanged;

    public bool IsListening
    {
        get
        {
            lock (_syncRoot)
            {
                return _isListening;
            }
        }
    }

    public VoiceCommandStartResult Start()
    {
        lock (_syncRoot)
        {
            if (_isDisposed)
            {
                return VoiceCommandStartResult.Failure("Voice input is unavailable.");
            }

            if (_isListening)
            {
                return VoiceCommandStartResult.Success();
            }

            try
            {
                _recognizer ??= CreateRecognizer();
                _recognizer.RecognizeAsync(RecognizeMode.Multiple);
                _isListening = true;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to start YellowPad voice input.");
                return VoiceCommandStartResult.Failure(
                    "Voice input is unavailable. Check your microphone and Windows speech settings.");
            }
        }

        ListeningChanged?.Invoke(this, EventArgs.Empty);
        return VoiceCommandStartResult.Success();
    }

    public void Stop()
    {
        SpeechRecognitionEngine? recognizer;
        lock (_syncRoot)
        {
            if (!_isListening)
            {
                return;
            }

            _isListening = false;
            recognizer = _recognizer;
        }

        try
        {
            recognizer?.RecognizeAsyncCancel();
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "YellowPad voice input had already stopped.");
        }

        ListeningChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        SpeechRecognitionEngine? recognizer;
        lock (_syncRoot)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _isListening = false;
            recognizer = _recognizer;
            _recognizer = null;
        }

        if (recognizer is null)
        {
            return;
        }

        recognizer.SpeechRecognized -= HandleSpeechRecognized;
        recognizer.RecognizeCompleted -= HandleRecognizeCompleted;
        recognizer.Dispose();
    }

    private SpeechRecognitionEngine CreateRecognizer()
    {
        var installedRecognizers = SpeechRecognitionEngine.InstalledRecognizers();
        var currentCulture = CultureInfo.CurrentUICulture;
        var recognizerInfo = installedRecognizers.FirstOrDefault(info => info.Culture.Equals(currentCulture))
            ?? installedRecognizers.FirstOrDefault(info =>
                info.Culture.TwoLetterISOLanguageName == currentCulture.TwoLetterISOLanguageName)
            ?? installedRecognizers.FirstOrDefault()
            ?? throw new InvalidOperationException("No Windows speech recognizer is installed.");

        var recognizer = new SpeechRecognitionEngine(recognizerInfo);
        try
        {
            recognizer.LoadGrammar(new DictationGrammar());
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.SpeechRecognized += HandleSpeechRecognized;
            recognizer.RecognizeCompleted += HandleRecognizeCompleted;
            return recognizer;
        }
        catch
        {
            recognizer.Dispose();
            throw;
        }
    }

    private void HandleSpeechRecognized(object? sender, SpeechRecognizedEventArgs args)
    {
        PhraseRecognized?.Invoke(this, new VoicePhraseRecognizedEventArgs(args.Result.Text));
    }

    private void HandleRecognizeCompleted(object? sender, RecognizeCompletedEventArgs args)
    {
        lock (_syncRoot)
        {
            if (!_isListening)
            {
                return;
            }

            _isListening = false;
        }

        if (args.Error is not null)
        {
            logger.LogWarning(args.Error, "YellowPad voice recognition stopped unexpectedly.");
        }

        ListeningChanged?.Invoke(this, EventArgs.Empty);
    }
}
