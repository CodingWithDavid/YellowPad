using YellowPad.Models;

namespace YellowPad.Services;

public interface ITodoService
{
    Task<IReadOnlyList<TodoItem>> GetTasksAsync();
    Task<TodoItem> AddTaskAsync(string title);
    Task UpdateTaskAsync(TodoItem item);
    Task ToggleCompletedAsync(int id);
    Task DeleteTaskAsync(int id);
    Task DeleteCompletedAsync();
    Task DeleteAllAsync();
}
