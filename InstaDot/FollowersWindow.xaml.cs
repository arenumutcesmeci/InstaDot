using InstagramApiSharp.API.Processors;
using InstagramApiSharp;
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
using InstagramApiSharp.Classes.Models;

namespace InstaDot
{
    /// <summary>
    /// Interaction logic for FollowersWindow.xaml
    /// </summary>
    public partial class FollowersWindow : UserControl
    {
        private MainWindow instance;
        private IUserProcessor _userProcessor;
        public FollowersWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            instance = mainWindow;
            _userProcessor = instance.Api.UserProcessor;
        }

        Dictionary<long, string> notFollowings;
        
        private async Task Load_Data()
        {
            RefreshBtn.Visibility = Visibility.Hidden;
            Unfollow.IsEnabled = false;
            UnfollowAll.IsEnabled = false;
            var _currentUser = await instance.Api.GetCurrentUserAsync();

            if (!_currentUser.Succeeded || _currentUser.Value == null)
            {
                instance.LoggedOut();
                RefreshBtn.Visibility = Visibility.Visible;
                return;
            }

            var followers = await _userProcessor.GetCurrentUserFollowersAsync(PaginationParameters.MaxPagesToLoad(int.MaxValue));
            var followings = await _userProcessor.GetUserFollowingAsync(_currentUser.Value.UserName, PaginationParameters.MaxPagesToLoad(int.MaxValue));
            notFollowings = followings.Value.Except(followers.Value).Select(x => new {Key = x.Pk, Value = x.UserName}).ToDictionary(x => x.Key, x => x.Value);
            NotFollowingList.ItemsSource = notFollowings.Select(x => x.Value).ToList();
            Unfollow.IsEnabled = true;
            UnfollowAll.IsEnabled = true;
            RefreshBtn.Visibility = Visibility.Visible;
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            await Load_Data();
        }

        private async void Unfollow_Click(object sender, RoutedEventArgs e)
        {
            RefreshBtn.Visibility = Visibility.Hidden;
            Unfollow.IsEnabled = false;
            UnfollowAll.IsEnabled = false;
            long? userId = notFollowings.Where(x => x.Value == NotFollowingList.SelectedValue).Select(x => x.Key).FirstOrDefault();
            if(userId != null)
            {
                var unfollowResponse = await _userProcessor.UnFollowUserAsync((long)userId);
                if(unfollowResponse.Succeeded)
                {
                    MessageBox.Show($"{NotFollowingList.SelectedValue} takipten çıkarıldı.", "Takipten çıkarıldı!");
                    notFollowings.Remove((long) userId);
                    NotFollowingList.ItemsSource = notFollowings;
                }else
                {
                    MessageBox.Show("Takipten çıkarılırken bir hata oluştu!", "Hata");
                }
                RefreshBtn.Visibility = Visibility.Visible;
                Unfollow.IsEnabled = true;
                UnfollowAll.IsEnabled = true;
                return;
            }
            MessageBox.Show("Takipten çıkarılırken bir hata oluştu!", "Hata");
            Unfollow.IsEnabled = true;
            UnfollowAll.IsEnabled = true;
            RefreshBtn.Visibility = Visibility.Visible;
        }

        private async void UnfollowAll_Click(object sender, RoutedEventArgs e)
        {
            RefreshBtn.Visibility = Visibility.Hidden;
            Unfollow.IsEnabled = false;
            UnfollowAll.IsEnabled = false;
            var unfollowed = new List<long>();
            foreach(var item in notFollowings)
            {
                var unfollowResult = await _userProcessor.UnFollowUserAsync(item.Key);
                if(unfollowResult.Succeeded)
                {
                    unfollowed.Add(item.Key);
                }
            }
            unfollowed.ForEach(x => notFollowings.Remove(x));
            NotFollowingList.ItemsSource = notFollowings;
            Unfollow.IsEnabled = true;
            UnfollowAll.IsEnabled = true;
            RefreshBtn.Visibility = Visibility.Visible;
        }

        private async void Followers_Loaded(object sender, RoutedEventArgs e)
        {
            await Load_Data();
        }
    }
}
