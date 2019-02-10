namespace RV.SubD.Core.Utils
{
    using System;
    using System.Net;
    using System.Text.RegularExpressions;

    public sealed class HttpUtility
    {
        public static string UrlEncode(string str)
        {
            return Regex.Replace(Uri.EscapeDataString(str), "%20", "+");
        }

        public static string UrlPathEncode(string str)
        {
            return Uri.EscapeDataString(str);
        }

        public static string HtmlDecode(string s)
        {
            return WebUtility.HtmlDecode(s);
        }

        public static string HtmlEncode(string s)
        {
            return WebUtility.HtmlEncode(s);
        }
    }
}
