using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DemoXamarin
{
    public static class CookieHelper
    {
        public static string AntiForgeryFieldName { get; } = "__RequestVerificationToken";
        public static string AntiForgeryCookieName { get; } = "AntiForgeryTokenCookie";

        public static bool IsExpiredCookie(IList<Cookie> cookies)
        {
            for (var i = 0; i < cookies.Count; i++)
            {
                if (cookies[i].Expires <= DateTime.Now)
                {
                    return true;
                }
            }

            return false;
        }

        public static IList<Cookie> CastToListCookies(CookieCollection cookies)
        {
            var cookiesResult = new List<Cookie>();
            for (var i = 0; i < cookies.Count; i++)
            {
                cookiesResult.Add(cookies[i]);
            }

            return cookiesResult;
        }

        public static async Task<(string fieldValue, string cookieValue)> ExtractAntiForgeryValues(HttpResponseMessage response)
        {
            var cookie = ExtractAntiForgeryCookieValueFrom(response);
            var token = ExtractAntiForgeryToken(await response.Content.ReadAsStringAsync());
            return (token, cookie);
        }

        private static string ExtractAntiForgeryToken(string htmlBody)
        {
            var requestVerificationTokenMatch = Regex.Match(htmlBody, $@"\<input name=""{AntiForgeryFieldName}"" type=""hidden"" value=""([^""]+)"" \/\>");
            if (requestVerificationTokenMatch.Success)
            {
                return requestVerificationTokenMatch.Groups[1].Captures[0].Value;
            }
            throw new ArgumentException($"Anti forgery token '{AntiForgeryFieldName}' not found in HTML", nameof(htmlBody));
        }

        private static string ExtractAntiForgeryCookieValueFrom(HttpResponseMessage response)
        {
            string antiForgeryCookie = response.Headers.GetValues("Set-Cookie").ToList()[0];

            if (antiForgeryCookie is null)
            {
                throw new ArgumentException($"Cookie '{AntiForgeryCookieName}' not found in HTTP response", nameof(response));
            }
            string antiForgeryCookieValue = SetCookieHeaderValue.Parse(antiForgeryCookie).Value.ToString();
            return antiForgeryCookieValue;
        }
    }
}
