using System.Diagnostics;
using System.Text;

namespace NotionFlow.App;

internal static class CrashLog
{
    public static string LogFilePath { get; } = ResolveLogPath();

    public static void Write(string source, object? error)
    {
        try
        {
            var sb = new StringBuilder();
            sb.Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("] ");
            sb.Append(source).AppendLine();
            if (error is Exception ex)
            {
                sb.AppendLine(ex.ToString());
            }
            else if (error is not null)
            {
                sb.AppendLine(error.ToString());
            }
            sb.AppendLine(new string('-', 60));

            var dir = Path.GetDirectoryName(LogFilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.AppendAllText(LogFilePath, sb.ToString());

            Debug.WriteLine(sb.ToString());
            Console.WriteLine(sb.ToString());
        }
        catch
        {
            // never throw from a crash logger
        }
    }

    private static string ResolveLogPath()
    {
        try
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(local))
                local = Path.GetTempPath();
            return Path.Combine(local, "NotionFlow", "startup-crash.log");
        }
        catch
        {
            return Path.Combine(Path.GetTempPath(), "NotionFlow-startup-crash.log");
        }
    }
}
