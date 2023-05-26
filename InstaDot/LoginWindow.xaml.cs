using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InstaDot
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : UserControl
    {
        MainWindow instance;
        string stateFileName = "state.bin";
        public LoginWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            instance = mainWindow;
            try
            {
                if(File.Exists(stateFileName))
                {
                    using var fs = File.OpenRead(stateFileName);
                    if (fs != null)
                    {
                        if(instance.Api == null)
                        {
                            instance.Api = InstaApiBuilder.CreateBuilder().UseLogger(new DebugLogger(LogLevel.Exceptions)).Build();
                        }

                        instance.Api.LoadStateDataFromStream(fs);
                        fs.Close();
                        if (instance.Api.IsUserAuthenticated)
                        {
                            instance.LoggedIn();
                        }
                    }
                }
            }catch
            {
                // IGNORE
            }
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginBtn.IsEnabled = false;
            var userSession = new UserSessionData
            {
                UserName = UsernameTxt.Text,
                Password = PasswordTxt.Password 
            };

            instance.Api = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .Build();

            if(!instance.Api.IsUserAuthenticated)
            {
                var loginResult = await instance.Api.LoginAsync();
                LoginBtn.IsEnabled = true;

                if(loginResult == null)
                {
                    MessageBox.Show("Fatal error", "Fatal error");
                    return;
                }


                if(!loginResult.Succeeded)
                {
                    var isLoggedIn = false;
                    if(loginResult.Value == InstaLoginResult.TwoFactorRequired)
                    {
                        while(!isLoggedIn)
                        {
                            var twoFactor = Microsoft.VisualBasic.Interaction.InputBox("İki aşamalı doğrulama", "İki aşamalı doğrulama");
                            if (twoFactor == null || twoFactor == "")
                                break;

                            var twoFactorloginResult = await instance.Api.TwoFactorLoginAsync(twoFactor);
                            isLoggedIn = twoFactorloginResult.Succeeded;
                        }
                    }
                    /*else if(loginResult.Value == InstaLoginResult.ChallengeRequired)
                    {
                        var challangeMethod = await instance.Api.GetChallengeRequireVerifyMethodAsync();
                        while(!isLoggedIn)
                        {
                            switch(challangeMethod)
                            {
                                
                            }
                        }
                    }*/
                    else
                    {
                        MessageBox.Show($"Giriş yapılırken bir hata oluştu!\nHata Mesajı:{loginResult.Info.Message}", "Hata");
                    }
                }
                else if(loginResult.Succeeded)
                {
                    var state = instance.Api.GetStateDataAsStream();
                    using (var fileStream = File.Create(stateFileName))
                    {
                        state.Seek(0, SeekOrigin.Begin);
                        state.CopyTo(fileStream);
                    }
                    instance.LoggedIn();
                }
            }
        }
    }
}
