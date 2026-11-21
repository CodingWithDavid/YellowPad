using YellowPad.Models;

namespace YellowPad.Services;

public interface ITabService
{
    Task<IReadOnlyList<TodoTab>> GetTabsAsync();
    Task<TodoTab> AddTabAsync(string name);
    Task DeleteTabAsync(int id);
}
