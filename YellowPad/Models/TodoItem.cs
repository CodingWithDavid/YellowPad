namespace YellowPad.Models;

public sealed class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? CompletedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public int SortOrder { get; set; }
}
