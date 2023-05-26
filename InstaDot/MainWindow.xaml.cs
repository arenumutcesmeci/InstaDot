using InstagramApiSharp.API;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Timers;

namespace InstaDot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IInstaApi Api { get; set; }
        private LoginWindow _loginWindow;
        private DashboardWindow? _dashboardWindow;
        private FollowersWindow? _followersWindow;
        private System.Timers.Timer _sessionCheckTimer;
        public MainWindow()
        {
            InitializeComponent();
            _sessionCheckTimer = new System.Timers.Timer();
            _sessionCheckTimer.Interval = 100;
            _sessionCheckTimer.Elapsed += SessionCheck;
            _loginWindow = new LoginWindow(this);
            if(Api == null || !Api.IsUserAuthenticated) {
                MainControl.Content = _loginWindow;
            }
        }

        private void SessionCheck(object sender, EventArgs e)
        {
            if(Api == null || !Api.IsUserAuthenticated)
            {
                LoggedOut();
                _sessionCheckTimer.Stop();
                MessageBox.Show("Session doldu.", "Çıkış yapıldı");
            }
        }

        public void LoggedIn()
        {
            if(Api.IsUserAuthenticated)
            {
                _sessionCheckTimer.Start();
                _dashboardWindow = new DashboardWindow(this);
                MainControl.Content = _dashboardWindow;
                UserNameLabel.Content = Api.GetLoggedUser().UserName;
                LogoutBtn.Visibility = Visibility.Visible;
                MenuControl.Visibility = Visibility.Visible;
            }else
            {
                MessageBox.Show("Giriş yapılırken bir hata oluştu.", "Hata");
            }
        }

        public async void LoggedOut()
        {
            if(Api.IsUserAuthenticated)
            {
                var logoutResult = await Api.LogoutAsync();
                if(logoutResult.Succeeded)
                {
                    if(File.Exists("state.bin"))
                    {
                        File.Delete("state.bin");
                    }
                    _sessionCheckTimer.Stop();
                    LogoutBtn.Visibility = Visibility.Collapsed;
                    UserNameLabel.Content = "Lütfen giriş yapınız";
                    MainControl.Content = _loginWindow;
                    MenuControl.Visibility = Visibility.Hidden;
                    if (_dashboardWindow != null)
                        _dashboardWindow = null;
                }
                else
                {
                    MessageBox.Show("Çıkış yaparken bir hata oluştu!", "Hata");
                }
            }
            else
            {
                MessageBox.Show("Zaten bir hesap açmadınız.", "Hata");
                LogoutBtn.Visibility = Visibility.Collapsed;
                UserNameLabel.Content = "Lütfen giriş yapınız";
                MainControl.Content = _loginWindow;
            }
        }

        private void AddMenuItem(string name, int id)
        {
            var menuButton = new Button();
            menuButton.Background = new SolidColorBrush(Colors.Transparent);
            menuButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
            menuButton.Foreground = new SolidColorBrush(Colors.White);
            menuButton.FontSize = 16;
            menuButton.Content = name;
            menuButton.Click += (sender, e) =>
            {
                SelectMenu(id);
            };
            MenuControl.Children.Add(menuButton);
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            AddMenuItem("Anasayfa", 0);
            AddMenuItem("Takipçi İşlemleri", 1);
            AddMenuItem("Toplu İşlemler", 2);
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            LoggedOut();
        }

        private void SelectMenu(int id)
        {
            switch(id)
            {
                case 0:
                    if (_dashboardWindow == null)
                        _dashboardWindow = new DashboardWindow(this);
                    MainControl.Content = _dashboardWindow;
                    break;
                case 1:
                    if (_followersWindow == null)
                        _followersWindow = new FollowersWindow(this);
                    MainControl.Content = _followersWindow;
                    break;
                default:
                    MainControl.Content = null;
                    break;
            }
        }
    }
}
