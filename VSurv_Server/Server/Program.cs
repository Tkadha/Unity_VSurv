using VSurvServer.Server.Hosting;

namespace VSurvServer.Server;

internal class Program
{
    static void Main(string[] args)
    {
        var serverHost = new ServerHost();
        serverHost.Start();

        Console.WriteLine("서버가 실행 중입니다. 종료하려면 Enter를 누르세요.");
        Console.ReadLine();

        serverHost.Stop();
    }
}