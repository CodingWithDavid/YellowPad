namespace YellowPad.Models;

public sealed class TodoTab
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public int SortOrder { get; set; }
}
