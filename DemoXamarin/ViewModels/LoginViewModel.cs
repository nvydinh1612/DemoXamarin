﻿using DemoXamarin.Commons;
using DemoXamarin.Models;
using DemoXamarin.Views;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Xamarin.Forms;

namespace DemoXamarin.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel()
        {
        }

        public UserModel User { get; set; }

        private string email;
        public string Email
        {
            get { return email; }
            set
            {
                email = value;
                OnPropertyChanged();
            }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                OnPropertyChanged();
            }
        }

        private string validateMessage;
        public string ValidateMessage
        {
            get
            {
                return validateMessage;
            }
            set
            {
                validateMessage = value;
                OnPropertyChanged();
            }
        }

        public Command LoginCommand
        {
            get
            {
                ValidateMessage = string.Empty;
                return new Command(Login);
            }
        }

        private async void Login()
        {
            var userModel = new UserModel
            {
                Email = Email,
                Password = Password
            };
            var validator = new UserModelValidator();
            var validationResult = validator.Validate(userModel);
            if (!validationResult.IsValid)
            {
                ValidateMessage = validationResult.Errors[0].ErrorMessage;
                return;
            }

            var cookieContainer = new CookieContainer();
            HttpClient httpClient = new HttpClient(new HttpClientHandler() { CookieContainer = cookieContainer });
            var initResponse = await httpClient.GetAsync(Contants.LOGIN_URL);

            var antiForgeryValues = await CookieHelper.ExtractAntiForgeryValues(initResponse);
            var postRequest = new HttpRequestMessage(HttpMethod.Post, Contants.LOGIN_URL);
            postRequest.Headers.Add("Cookie", new CookieHeaderValue(CookieHelper.AntiForgeryCookieName, antiForgeryValues.cookieValue).ToString());
            var modelData = new Dictionary<string, string>
                {
                    { CookieHelper.AntiForgeryFieldName, antiForgeryValues.fieldValue },
                    { "Username", this.Email},
                    { "Password", this.Password }
                };
            postRequest.Content = new FormUrlEncodedContent(modelData);

            await httpClient.SendAsync(postRequest);
            var cookies = cookieContainer.GetCookies(new System.Uri(Contants.LOGIN_URL));
            if (cookies.Count <= 1)
            {
                App.Current.MainPage.DisplayAlert("Login Failed", "Invalid Email or Password. Please try again!", "OK");
                return;
            }

            UserInfo.CookieContainer = cookieContainer;
            await App.Current.MainPage.Navigation.PushModalAsync(new MainPageView());
        }
    }
}
