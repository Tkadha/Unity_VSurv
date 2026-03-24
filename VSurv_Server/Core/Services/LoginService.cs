using System;
using System.Data;
using Dapper;
using VSurvServer.Core.Utils;
using VSurvServer.Infrastructure.Database;
using VSurvServer.Infrastructure.Logging;
using VSurvServer.Protocol.Packets;

namespace VSurvServer.Core.Services;

public class LoginService
{
    public LoginResponse Handle(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResponse { Success = false, Message = "아이디와 비밀번호를 입력해주세요." };
        }

        try
        {
            using (IDbConnection db = DatabaseManager.GetConnection())
            {
                db.Open();

                string sql = "SELECT id, password_hash FROM users WHERE username = @Username";

                var user = db.QueryFirstOrDefault(sql, new { Username = request.Username });

                if (user == null)
                {
                    return new LoginResponse { Success = false, Message = "존재하지 않는 아이디입니다." };
                }

                string inputHash = CryptoUtils.HashPassword(request.Password);

                if (inputHash != user.password_hash)
                {
                    return new LoginResponse { Success = false, Message = "비밀번호가 일치하지 않습니다." };
                }

                return new LoginResponse
                {
                    Success = true,
                    Message = "로그인 성공",
                    UserId = user.id 
                };
            }
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"로그인 처리 중 DB 오류: {ex.Message}");
            return new LoginResponse { Success = false, Message = "서버 내부 오류로 로그인에 실패했습니다." };
        }
    }
}