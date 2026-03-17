using VSurvServer.Infrastructure.Logging;

namespace VSurvServer.Server.Hosting;

public class ServerHost
{
    public void Start()
    {
        ServerLogger.Info("ServerHost.Start 호출");
        ServerLogger.Info("서버 초기화 시작");

        // TODO: 리스너 생성
        // TODO: 세션 관리자 연결
        // TODO: 패킷 처리기 연결

        ServerLogger.Info("서버 초기화 완료");
    }

    public void Stop()
    {
        ServerLogger.Info("ServerHost.Stop 호출");
        ServerLogger.Info("서버 종료 처리");
    }
}