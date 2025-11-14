using Microsoft.EntityFrameworkCore;
using pleer.Models.DatabaseContext;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace pleer.Resources.Pages.GeneralPages
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;
        ArtistMainWindow _artistMain;

        Listener _listener;
        Artist _artist;

        public LoginPage(ListenerMainWindow main)
        {
            InitializeComponent();

            _listenerMain = main;

            UserInactiveGrid();
        }

        public LoginPage(ArtistMainWindow main)
        {
            InitializeComponent();

            _artistMain = main;

            ArtistInactiveGrid();
        }

        void UserInactiveGrid()
        {
            _listenerMain.FullWindow.Visibility = Visibility.Visible;
            _listenerMain.WindowBlurEffect.Radius = 15;
            _listenerMain.MainGrid.IsEnabled = false;
        }

        void UserActiveGrid()
        {
            _listenerMain.FullWindow.Visibility = Visibility.Collapsed;
            _listenerMain.WindowBlurEffect.Radius = 0;
            _listenerMain.MainGrid.IsEnabled = true;
        }

        void ArtistInactiveGrid()
        {
            _artistMain.FullWindow.Visibility = Visibility.Visible;
            _artistMain.WindowBlurEffect.Radius = 15;
            _artistMain.MainGrid.IsEnabled = false;
        }

        void ArtistActiveGrid()
        {
            _artistMain.FullWindow.Visibility = Visibility.Collapsed;
            _artistMain.WindowBlurEffect.Radius = 0;
            _artistMain.MainGrid.IsEnabled = true;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckUserDataValid())
                return;

            var isEmailValid = ServiceMethods.IsValidEmail(UserEmail.Text);
            if (!isEmailValid)
            {
                ErrorNoticePanel.Text = "Неверный формат почты";
                return;
            }
            else ErrorNoticePanel.Text = string.Empty;


            var password = ServiceMethods.IsPasswordsValidOutput(UserPassword.Text);
            if (password != UserPassword.Text)
            {
                ErrorNoticePanel.Text = password;
                return;
            }
            else ErrorNoticePanel.Text = string.Empty;

            try
            {
                if (_listenerMain != null)
                {
                    _listener = _context.Listeners
                        .FirstOrDefault(u => u.Email == UserEmail.Text);

                    if (_listener != null)
                    {
                        var pass = ServiceMethods.GetSha256Hash(password);

                        if (_listener.PasswordHash != pass)
                        {
                            ErrorNoticePanel.Text = "Неверный пароль";
                            return;
                        }
                        else ErrorNoticePanel.Text = string.Empty;

                        await OpenNewWindow(_listener, _artist);
                    }
                    else
                    {
                        ErrorNoticePanel.Text = "Пользователь не найден";
                        return;
                    }
                }

                if (_artistMain != null)
                {
                    _artist = _context.Artists
                        .FirstOrDefault(a => a.Email == UserEmail.Text);

                    if (_artist != default)
                    {
                        var passwordHash = ServiceMethods.GetSha256Hash(password);

                        if (_artist.PasswordHash != passwordHash)
                        {
                            ErrorNoticePanel.Text = "Неверный пароль";
                            return;
                        }

                        await OpenNewWindow(_listener, _artist);
                    }
                    else
                    {
                        ErrorNoticePanel.Text = "Исполнитель не найден";
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex}");
            }
        }

        async Task OpenNewWindow(Listener listener, Artist artist)
        {
            if (artist != null)
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    var newWindow = new ArtistMainWindow(artist);
                    Application.Current.MainWindow = newWindow;

                    newWindow.Show();
                    _artistMain.Close();
                }), DispatcherPriority.Background);
            }

            if (listener != null)
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    var newWindow = new ListenerMainWindow(listener);
                    Application.Current.MainWindow = newWindow;

                    newWindow.Show();
                    _listenerMain.Close();
                }), DispatcherPriority.Background);
            }
        }

        bool CheckUserDataValid()
        {
            if (string.IsNullOrEmpty(UserEmail.Text) ||
                string.IsNullOrEmpty(UserPassword.Text))
            {
                ErrorNoticePanel.Text = "Заполните все необходимые поля";
                return false;
            }
            else
                return true;
        }

        private void TurnToRegistration_Click(object sender, MouseButtonEventArgs e)
        {
            if (_listenerMain != null)
                _listenerMain.FullWindow.Navigate(new RegistrationPage(_listenerMain));
            if (_artistMain != null)
                _artistMain.FullWindow.Navigate(new RegistrationPage(_artistMain));

        }

        private void CloseFullWindowFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (_listenerMain != null)
                UserActiveGrid();
            if (_artistMain != null)
                ArtistActiveGrid();
        }
    }
}
