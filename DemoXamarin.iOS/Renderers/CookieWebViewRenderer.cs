using DemoXamarin.Commons;
using DemoXamarin.Controls;
using DemoXamarin.iOS.Renderers;
using DemoXamarin.Models;
using DemoXamarin.Views;
using Foundation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CookieWebView), typeof(CookieWebViewRenderer))]
namespace DemoXamarin.iOS.Renderers
{
    public class CookieWebViewRenderer : WkWebViewRenderer
    {
        public CookieWebViewRenderer()
        {
        }

        protected async override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cookie.json");

            if (!File.Exists(fileName) && UserInfo.CookieContainer.Count < 1)
            {
                await App.Current.MainPage.Navigation.PushModalAsync(new LoginView());
                return;
            }

            var currentCookies = File.Exists(fileName) ? JsonConvert.DeserializeObject<IList<Cookie>>(File.ReadAllText(fileName))
                : CookieHelper.CastToListCookies(UserInfo.CookieContainer.GetCookies(new System.Uri(Contants.INDEX_URL)));

            if (File.Exists(fileName) && CookieHelper.IsExpiredCookie(currentCookies))
            {
                File.Delete(fileName);
                await App.Current.MainPage.Navigation.PushModalAsync(new LoginView());
                return;
            }

            var cookieUrl = new Uri(Contants.INDEX_URL);
            var cookieJar = NSHttpCookieStorage.SharedStorage;
            cookieJar.AcceptPolicy = NSHttpCookieAcceptPolicy.Always;

            foreach (var aCookie in cookieJar.Cookies)
            {
                cookieJar.DeleteCookie(aCookie);
            }

            var jCookies = UserInfo.CookieContainer.GetCookies(cookieUrl);
            IList<NSHttpCookie> eCookies =
                (from object jCookie in jCookies
                 where jCookie != null
                 select (Cookie)jCookie
                 into netCookie
                 select new NSHttpCookie(netCookie)).ToList();
            cookieJar.SetCookies(eCookies.ToArray(), cookieUrl, cookieUrl);

            if (!File.Exists(fileName))
            {
                var jsonToOutput = JsonConvert.SerializeObject(currentCookies);
                File.WriteAllText(fileName, jsonToOutput);
            }

            this.LoadUrl(Contants.INDEX_URL);
        }
    }
}