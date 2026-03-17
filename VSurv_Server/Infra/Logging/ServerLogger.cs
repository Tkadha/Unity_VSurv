namespace VSurvServer.Infrastructure.Logging;

public static class ServerLogger
{
    public static void Info(string message)
    {
        Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}");
    }

    public static void Error(string message)
    {
        Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}");
    }
}