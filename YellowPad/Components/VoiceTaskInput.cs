namespace YellowPad.Components;

public static class VoiceTaskInput
{
    public const int MaximumTitleLength = 500;

    private static readonly string[] CreateCommands =
    [
        "create new task",
        "create task",
        "enter",
    ];

    private static readonly char[] CommandPunctuation = ['.', ',', '!', '?', ';', ':'];

    public static VoiceTaskInputResult Interpret(string currentTitle, string recognizedPhrase)
    {
        var title = currentTitle?.Trim() ?? string.Empty;
        var phrase = recognizedPhrase?.Trim() ?? string.Empty;
        if (TryRemoveCreateCommand(phrase, out var dictatedTitle))
        {
            return new(Append(title, dictatedTitle), ShouldCreate: true);
        }

        if (phrase.Length == 0)
        {
            return new(title, ShouldCreate: false);
        }

        return new(Append(title, phrase), ShouldCreate: false);
    }

    private static bool TryRemoveCreateCommand(string phrase, out string dictatedTitle)
    {
        var withoutPunctuation = phrase.TrimEnd(CommandPunctuation).TrimEnd();
        foreach (var command in CreateCommands)
        {
            if (string.Equals(withoutPunctuation, command, StringComparison.OrdinalIgnoreCase))
            {
                dictatedTitle = string.Empty;
                return true;
            }

            if (withoutPunctuation.EndsWith($" {command}", StringComparison.OrdinalIgnoreCase))
            {
                dictatedTitle = withoutPunctuation[..^(command.Length + 1)]
                    .TrimEnd(CommandPunctuation)
                    .TrimEnd();
                return true;
            }
        }

        dictatedTitle = string.Empty;
        return false;
    }

    private static string Append(string title, string phrase)
    {
        var combined = string.Join(' ', new[] { title, phrase }.Where(part => part.Length > 0));
        return combined[..Math.Min(combined.Length, MaximumTitleLength)];
    }
}

public readonly record struct VoiceTaskInputResult(string Title, bool ShouldCreate);
