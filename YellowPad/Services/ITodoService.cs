using YellowPad.Models;

namespace YellowPad.Services;

public interface ITodoService
{
    Task<IReadOnlyList<TodoItem>> GetTasksAsync(int tabId);
    Task<TodoItem> AddTaskAsync(int tabId, string title);
    Task UpdateTaskAsync(TodoItem item);
    Task SetTaskColorAsync(int id, TaskColor color);
    Task ToggleCompletedAsync(int id);
    Task DeleteTaskAsync(int id);
    Task DeleteCompletedAsync(int tabId);
    Task DeleteAllAsync(int tabId);
}
