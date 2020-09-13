using System;
using System.Net;
using Xamarin.Forms;

namespace DemoXamarin.Controls
{
    public class CookieWebView: Xamarin.Forms.WebView
    {
        public static readonly BindableProperty CookiesProperty = BindableProperty.Create(
        propertyName: "Cookies",
            returnType: typeof(CookieContainer),
            declaringType: typeof(CookieWebView),
          defaultValue: default(string));
    }
}
