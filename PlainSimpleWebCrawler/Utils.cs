

using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace PlainSimpleWebCrawler
{
    public static class Utils
    {
        private const string UrlHttps = @"https://";
        private const string UrlHttp = @"http://";
        private const string UrlWwww = @"www.";
        public static bool IsNullOrEmpty(this string param)
        {
            return string.IsNullOrEmpty(param != null ? param.Trim() : null);
        }



        public static string ReplaceFirstOccurrence(this string source, string find, string replace)
        {
            var place = source.IndexOf(find, StringComparison.InvariantCultureIgnoreCase);
            var result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }
        public static string UrlFixParamers(this string url)
        {
            if (url.Contains("&") && !url.Contains("?"))
                url = url.ReplaceFirstOccurrence("&", "?");
            return url;
        }

        public static string Md5Get(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;
            var x = new MD5CryptoServiceProvider();
            var bs = Encoding.UTF8.GetBytes(str);
            bs = x.ComputeHash(bs);
            var s = new StringBuilder();
            foreach (var b in bs)
                s.Append(b.ToString("x2").ToLower());
            var password = s.ToString();
            return password;
        }
        public static Uri ToUri(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            Uri result;
            return Uri.TryCreate(str.WithHttp(), UriKind.RelativeOrAbsolute, out result) ? result : null;
        }
        public static string WithHttp(this string url)
        {
            if (url == null)
                return string.Empty;

            if (url.Contains(UrlHttps) || url.Contains(UrlHttp))
                return url;
            if (url.StartsWith("//"))
                url = url.Replace("//", string.Empty);
            return WithStarts(url, UrlHttp);
        }
        public static string UrlFixUrl(this string url)
        {
            return string.IsNullOrEmpty(url) ? string.Empty : url.Replace("\\", @"\").Replace(@"\", @"/").UrlFixParamers();
        }

        public static string WithStarts(this string url, string sign)
        {
            if (url.IsNullOrEmpty() || url.Trim().IsNullOrEmpty())
                return string.Empty;

            url = url.UrlFixUrl();
            var temp = url;
            var prefix = temp.StartsWith(sign, StringComparison.OrdinalIgnoreCase) ? string.Empty : sign;

            return prefix.IsNullOrEmpty() ? url : $"{sign}{url}";
        }
        public static string UrlDecode(this string param, bool totallyDecodeFromGoogleFormat = false)
        {
            var result = HttpUtility.UrlDecode(param ?? string.Empty);
            if (!totallyDecodeFromGoogleFormat)
                return result;

            if (result != null)
            {
                result = result.Replace("&amp;rct", "?rct");
                result = result.Replace("&amp;", "&");
            }
            return result;
        }
        public static string UrlGetDomain(this string url)
        {
            url = url.UrlDecode().UrlFixUrl().UrlFixParamers();
            Uri uri;
            if (Uri.TryCreate(url.WithHttp(), UriKind.RelativeOrAbsolute, out uri))
                return uri.IsAbsoluteUri
                    ? uri.GetComponents(UriComponents.NormalizedHost, UriFormat.SafeUnescaped).Without3W()
                    : string.Empty;

            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
                return uri.IsAbsoluteUri
                    ? uri.GetComponents(UriComponents.NormalizedHost, UriFormat.SafeUnescaped).Without3W()
                    : string.Empty;
            return string.Empty;
        }

        public static string Without3W(this string url1)
        {
            return Without(url1, UrlWwww);
        }
        public static string Without(this string url, string sign)
        {
            if (url.IsNullOrEmpty())
                return string.Empty;

            var result = url;
            var prefix = result.Contains(sign) ? sign : string.Empty;

            return prefix.IsNullOrEmpty() ? result : result.Replace(prefix, string.Empty);
        }
    }
}
