using System.Security.Cryptography;
using System.Text;

namespace VSurvServer.Core.Utils;

public static class CryptoUtils
{
    // 단순 SHA256은 레인보우 테이블 공격에 취약
    // 고정된 Salt 값을 문자열에 추가하여 해싱하는 방식
    private const string GlobalSalt = "VSurv_Secret_Key";

    public static string HashPassword(string plainPassword)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            string saltedPassword = plainPassword + GlobalSalt;
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}