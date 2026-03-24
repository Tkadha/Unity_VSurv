namespace VSurvServer.Protocol.Packets;

public class StartGameRequest
{
    public PacketId PacketId => PacketId.StartGameRequest;

    public string PlayerName { get; set; } = string.Empty;
}

public class StartGameResponse
{
    public PacketId PacketId => PacketId.StartGameResponse;

    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
public class PingRequest
{
    public PacketId PacketId => PacketId.PingRequest;

    public long ClientTimeTicks { get; set; }
}

public class PingResponse
{
    public PacketId PacketId => PacketId.PingResponse;

    public bool Success { get; set; }
    public long ClientTimeTicks { get; set; }
    public long ServerTimeTicks { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class EndGameRequest
{
    public PacketId PacketId => PacketId.EndGameRequest;
}

public class EndGameResponse
{
    public PacketId PacketId => PacketId.EndGameResponse;
    public bool Success { get; set; }
}

// --- 회원가입 ---
public class RegisterRequest
{
    public PacketId PacketId => PacketId.RegisterRequest;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // 클라이언트는 평문 전송, 서버가 해싱
}

public class RegisterResponse
{
    public PacketId PacketId => PacketId.RegisterResponse;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

// --- 로그인 ---
public class LoginRequest
{
    public PacketId PacketId => PacketId.LoginRequest;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public PacketId PacketId => PacketId.LoginResponse;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; } // 성공 시 발급받는 고유 유저 번호
}