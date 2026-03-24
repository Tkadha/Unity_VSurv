using System;
using System.Data;
using Dapper;
using VSurvServer.Core.Utils;
using VSurvServer.Infrastructure.Database;
using VSurvServer.Infrastructure.Logging;
using VSurvServer.Protocol.Packets;

namespace VSurvServer.Core.Services;

public class RegisterService
{
    public RegisterResponse Handle(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new RegisterResponse { Success = false, Message = "아이디와 비밀번호를 모두 입력해주세요." };
        }

        try
        {
            using (IDbConnection db = DatabaseManager.GetConnection())
            {
                db.Open();

                string checkSql = "SELECT COUNT(1) FROM users WHERE username = @Username";
                int count = db.ExecuteScalar<int>(checkSql, new { Username = request.Username });

                if (count > 0)
                {
                    return new RegisterResponse { Success = false, Message = "이미 존재하는 아이디입니다." };
                }

                string hashedPassword = CryptoUtils.HashPassword(request.Password);

                string insertSql = @"
                    INSERT INTO users (username, password_hash) 
                    VALUES (@Username, @PasswordHash)";

                db.Execute(insertSql, new { Username = request.Username, PasswordHash = hashedPassword });

                return new RegisterResponse { Success = true, Message = "회원가입이 성공적으로 완료되었습니다." };
            }
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"회원가입 처리 중 DB 오류: {ex.Message}");
            return new RegisterResponse { Success = false, Message = "서버 내부 오류로 회원가입에 실패했습니다." };
        }
    }
}