using Android.Content;
using Android.Webkit;
using DemoXamarin.Commons;
using DemoXamarin.Controls;
using DemoXamarin.Droid.Renderers;
using DemoXamarin.Models;
using DemoXamarin.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Cookie = System.Net.Cookie;

[assembly: ExportRenderer(typeof(CookieWebView), typeof(CookieWebViewRenderer))]
namespace DemoXamarin.Droid.Renderers
{
    public class CookieWebViewRenderer : WebViewRenderer
    {
        Context _context;

        public CookieWebViewRenderer(Context context) : base(context)
        {
            _context = context;
        }

        protected async override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);

            var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cookie.json");

            if (!File.Exists(fileName) && UserInfo.CookieContainer.Count < 1)
            {
                await App.Current.MainPage.Navigation.PushModalAsync(new LoginView());
                return;
            }

            var currentCookies = File.Exists(fileName) ? JsonConvert.DeserializeObject<IList<Cookie>>(File.ReadAllText(fileName)) 
                : CastToListCookies(UserInfo.CookieContainer.GetCookies(new System.Uri(Contants.INDEX_URL)));

            if (File.Exists(fileName) && IsExpiredCookie(currentCookies))
            {
                File.Delete(fileName);
                await App.Current.MainPage.Navigation.PushModalAsync(new LoginView());
                return;
            }

            var cookieManager = CookieManager.Instance;
            cookieManager.SetAcceptCookie(true);
            cookieManager.RemoveAllCookie();

            for (var i = 0; i < currentCookies.Count; i++)
            {
                string cookieValue = currentCookies[i].Value;
                string cookieName = currentCookies[i].Name;
                currentCookies[i].Expires = currentCookies[i].TimeStamp.AddMinutes(Contants.EXPIRED_TIME);
                cookieManager.SetCookie(Contants.DOMAIN_URL, $"{cookieName}={cookieValue}");
            }
            if (!File.Exists(fileName))
            {
                var jsonToOutput = JsonConvert.SerializeObject(currentCookies);
                File.WriteAllText(fileName, jsonToOutput);
            }
            
            this.LoadUrl(Contants.INDEX_URL);
        }

        private bool IsExpiredCookie(IList<Cookie> cookies)
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

        private IList<Cookie> CastToListCookies(CookieCollection cookies)
        {
            var cookiesResult = new List<Cookie>();
            for (var i = 0; i < cookies.Count; i++)
            {
                cookiesResult.Add(cookies[i]);
            }

            return cookiesResult;
        }
    }
}