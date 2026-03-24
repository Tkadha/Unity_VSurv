using System;

[Serializable]
public class StartGameRequest { public string PlayerName; }
[Serializable]
public class StartGameResponse { public bool Success; public string Message; }

[Serializable]
public class PingRequest { public long ClientTimeTicks; }
[Serializable]
public class PingResponse { public bool Success; public long ClientTimeTicks; public long ServerTimeTicks; public string Message; }

[Serializable]
public class EndGameRequest { }
[Serializable]
public class EndGameResponse { public bool Success; }

// --- 회원가입 ---
[Serializable]
public class RegisterRequest
{
    public string Username;
    public string Password;
}
[Serializable]
public class RegisterResponse
{
    public bool Success;
    public string Message;
}

// --- 로그인 ---
[Serializable]
public class LoginRequest
{
    public string Username;
    public string Password;
}
[Serializable]
public class LoginResponse
{
    public bool Success;
    public string Message;
    public int UserId;
}