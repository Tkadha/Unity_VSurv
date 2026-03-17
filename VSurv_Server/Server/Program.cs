using VSurvServer.Server.Hosting;

namespace VSurvServer.Server;

internal class Program
{
    static void Main(string[] args)
    {
        var serverHost = new ServerHost();
        serverHost.Start();

        Console.WriteLine("서버를 종료하려면 아무 키나 누르세요.");
        Console.ReadKey();

        serverHost.Stop();
    }
}