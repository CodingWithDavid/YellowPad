using System.IO;

namespace YellowPad.Data;

public static class DatabasePathProvider
{
    public static string GetDatabasePath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var directory = Path.Combine(localAppData, "YellowPad");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "yellowpad.db");
    }
}
