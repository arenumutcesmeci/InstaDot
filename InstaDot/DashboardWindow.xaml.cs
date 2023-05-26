using InstagramApiSharp;
using InstagramApiSharp.API.Processors;
using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for DashboardWindow.xaml
    /// </summary>
    public partial class DashboardWindow : UserControl
    {
        private MainWindow instance;
        private IUserProcessor _userProcessor;
        private IAccountProcessor _accountProcessor;
        public DashboardWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            instance = mainWindow;
            if(instance.Api == null || !instance.Api.IsUserAuthenticated)
            {
                instance.LoggedOut();
                return;
            }
            _userProcessor = instance.Api.UserProcessor;
            _accountProcessor = instance.Api.AccountProcessor;
        }

        private async Task Load_Data()
        {
            RefreshBtn.Visibility = Visibility.Hidden;
            var _currentUser = await instance.Api.GetCurrentUserAsync();

            if (!_currentUser.Succeeded || _currentUser.Value == null)
            {
                instance.LoggedOut();
                RefreshBtn.Visibility = Visibility.Visible;
                return;
            }

            var followers = await _userProcessor.GetCurrentUserFollowersAsync(PaginationParameters.MaxPagesToLoad(int.MaxValue));
            var followings = await _userProcessor.GetUserFollowingAsync(_currentUser.Value.UserName, PaginationParameters.MaxPagesToLoad(int.MaxValue));
            if (_currentUser != null)
            {
                UsernameTxt.Content = $"@{_currentUser.Value.UserName}";
                UsernameBox.Text = _currentUser.Value.UserName;
                FullnameTxt.Content = $"{_currentUser.Value.FullName}";
                FullnameBox.Text = _currentUser.Value.FullName;
                IsUserPrivate.Visibility = _currentUser.Value.IsPrivate ? Visibility.Visible : Visibility.Collapsed;
                IsVerified.Visibility = _currentUser.Value.IsVerified ? Visibility.Visible : Visibility.Collapsed;
                FollowersTxt.Content = $"Takipçi Sayısı: {followers.Value.Count}";
                FollowingsTxt.Content = $"Takip Sayısı: {followings.Value.Count}";
                ProfilePicture.Source = new BitmapImage(new Uri(_currentUser.Value.HdProfilePicture.Uri));
            }
            RefreshBtn.Visibility = Visibility.Visible;
        }

        private async void Dashboard_Loaded(object sender, RoutedEventArgs e)
        {
            await Load_Data();
        }

        private async void SaveUserSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var requestForEdit = await _accountProcessor.GetRequestForEditProfileAsync();
            if(!requestForEdit.Succeeded) {
                MessageBox.Show("Ayarları kaydederken bir hata oluştu!", "Hata!");
                return;
            }

            if(UsernameTxt.Content.ToString() != UsernameBox.Text)
            {
                var usernameResult = await _accountProcessor.CheckUsernameAsync(UsernameBox.Text);
                if (!usernameResult.Succeeded)
                {
                    MessageBox.Show("Bu kullanıcı adını alamazsınız!", "Hata");
                    return;
                }
                requestForEdit.Value.Username = UsernameBox.Text;
            }

            if(FullnameTxt.Content.ToString() !=  FullnameBox.Text)
            {
                requestForEdit.Value.FullName = FullnameBox.Text;
            }
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            await Load_Data();
        }
    }
}
