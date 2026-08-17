using YellowPad.Components;

namespace YellowPad.Tests;

public sealed class VoiceTaskInputTests
{
    [Theory]
    [InlineData("Enter")]
    [InlineData("create new task.")]
    public void Interpret_RecognizesCreateCommands(string phrase)
    {
        var result = VoiceTaskInput.Interpret("Buy milk", phrase);

        Assert.True(result.ShouldCreate);
        Assert.Equal("Buy milk", result.Title);
    }

    [Fact]
    public void Interpret_AppendsDictationToTheCurrentTitle()
    {
        var result = VoiceTaskInput.Interpret("Call", "the dentist");

        Assert.False(result.ShouldCreate);
        Assert.Equal("Call the dentist", result.Title);
    }

    [Theory]
    [InlineData("", "Buy milk enter", "Buy milk")]
    [InlineData("Call", "Jane, create new task.", "Call Jane")]
    public void Interpret_CreatesWhenTheCommandEndsTheDictatedPhrase(
        string currentTitle,
        string phrase,
        string expectedTitle)
    {
        var result = VoiceTaskInput.Interpret(currentTitle, phrase);

        Assert.True(result.ShouldCreate);
        Assert.Equal(expectedTitle, result.Title);
    }

    [Fact]
    public void Interpret_LimitsDictationToTheTaskTitleLength()
    {
        var result = VoiceTaskInput.Interpret(new string('a', 499), "bc");

        Assert.Equal(500, result.Title.Length);
    }
}
