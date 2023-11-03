using System;
using System.Text;

namespace NewsReader.Models
{
    public class Helper
    {
        public static string DecodeUtf8String(string encodedString)
        {
            string decodedString = encodedString
                .Replace("=C3=B8", "ø")
                .Replace("=C3=A6", "æ")
                .Replace("=C3=A5", "å")
                .Replace("=20", " ")
                .Replace("=C2=A0", " ")
                .Replace("=E2=80=99", "'")
                .Replace("=C2=AB", "«")
                .Replace("=C2=BB", "»");

            return decodedString;
        }
    }
}