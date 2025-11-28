using pleer.Models.DatabaseContext;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
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
        AdminMainWindow _adminMain;

        Listener _listener;
        Artist _artist;
        Admin _admin;

        public LoginPage(ListenerMainWindow main)
        {
            InitializeComponent();

            _listenerMain = main;

            _listener = _context.Listeners.First();
            OpenNewWindow(_listener);

            UserInactiveGrid();
        }

        public LoginPage(ArtistMainWindow main)
        {
            InitializeComponent();

            _artistMain = main;

            _artist = _context.Artists.First();
            OpenNewWindow(_artist);

            ArtistInactiveGrid();
        }

        public LoginPage(AdminMainWindow main)
        {
            InitializeComponent();

            _adminMain = main;

            _admin = _context.Admins.First();
            OpenNewWindow(_admin);

            CloseFullWindowFrameButton.Visibility = Visibility.Collapsed;
            TurnToRegistration.Visibility = Visibility.Collapsed;

            EmailTextBlock.Text = "Логин";

            AdminInactiveGrid();
        }

        void UserInactiveGrid()
        {
            _listenerMain.FullWindow.Visibility = Visibility.Visible;
            _listenerMain.MainGrid.Effect = new BlurEffect { Radius = 15, KernelType = KernelType.Gaussian };
            _listenerMain.MainGrid.IsEnabled = false;
        }

        void UserActiveGrid()
        {
            _listenerMain.FullWindow.Visibility = Visibility.Collapsed;
            _listenerMain.MainGrid.Effect = null;
            _listenerMain.MainGrid.IsEnabled = true;
        }

        void ArtistInactiveGrid()
        {
            _artistMain.FullWindow.Visibility = Visibility.Visible;
            _artistMain.MainGrid.Effect = new BlurEffect { Radius = 15, KernelType = KernelType.Gaussian };
            _artistMain.MainGrid.IsEnabled = false;
        }

        void ArtistActiveGrid()
        {
            _artistMain.OperationField.Visibility = Visibility.Collapsed;
            _artistMain.MainGrid.Effect = null;
            _artistMain.MainGrid.IsEnabled = true;
        }

        void AdminInactiveGrid()
        {
            _adminMain.MainGrid.Effect = new BlurEffect { Radius = 15, KernelType = KernelType.Gaussian };
            _adminMain.MainGrid.IsEnabled = false;
            _adminMain.BackButton.IsEnabled = false; _adminMain.ForwardButton.IsEnabled = false;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckUserDataValid())
                return;

            string password = string.Empty;

            if (_adminMain == null)
            {
                var isEmailValid = ServiceMethods.IsValidEmail(UserEmail.Text);
                if (!isEmailValid)
                {
                    ErrorNoticePanel.Text = "Неверный формат почты";
                    return;
                }
                else ErrorNoticePanel.Text = string.Empty;

                password = ServiceMethods.IsPasswordsValidOutput(UserPassword.Text);
                if (password != UserPassword.Text)
                {
                    ErrorNoticePanel.Text = password;
                    return;
                }
                else ErrorNoticePanel.Text = string.Empty;
            }

            try
            {
                if (_listenerMain != null)
                {
                    _listener = _context.Listeners
                        .FirstOrDefault(u => u.Email == UserEmail.Text);

                    if (_listener.Status)
                    {
                        ErrorNoticePanel.Text = "Ваш аккаунт был временно заблокирован, дождитесь разблокировки или обратитесь в поддержку (ее кстати нет))";
                        return;
                    }

                    if (_listener != null)
                    {
                        var pass = ServiceMethods.GetSha256Hash(password);

                        if (_listener.PasswordHash != pass)
                        {
                            ErrorNoticePanel.Text = "Неверный пароль";
                            return;
                        }
                        else ErrorNoticePanel.Text = string.Empty;

                        await OpenNewWindow(_listener);
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

                    if (_artist.Status)
                    {
                        ErrorNoticePanel.Text = "Ваш аккаунт был временно заблокирован, дождитесь разблокировки или обратитесь в поддержку (ее кстати нет))";
                        return;
                    }

                    if (_artist != default)
                    {
                        var passwordHash = ServiceMethods.GetSha256Hash(password);

                        if (_artist.PasswordHash != passwordHash)
                        {
                            ErrorNoticePanel.Text = "Неверный пароль";
                            return;
                        }

                        await OpenNewWindow(_artist);
                    }
                    else
                    {
                        ErrorNoticePanel.Text = "Исполнитель не найден";
                        return;
                    }
                }

                if (_adminMain != null)
                {
                    _admin = _context.Admins
                        .FirstOrDefault(a => a.Login == UserEmail.Text);

                    if (_admin != default)
                    {
                        var passwordHash = ServiceMethods.GetSha256Hash(password);

                        if (_admin.PasswordHash != passwordHash)
                        {
                            ErrorNoticePanel.Text = "Неверный пароль";
                            return;
                        }

                        await OpenNewWindow(_admin);
                    }
                    else
                    {
                        ErrorNoticePanel.Text = "Администратор не найден";
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        async Task OpenNewWindow(Listener listener)
        {
            await Dispatcher.BeginInvoke(new Action(() =>
            {
                var newWindow = new ListenerMainWindow(listener);
                Application.Current.MainWindow = newWindow;

                newWindow.Show();
                _listenerMain.Close();
            }), DispatcherPriority.Background);
        }

        async Task OpenNewWindow(Artist artist)
        {
            await Dispatcher.BeginInvoke(new Action(() =>
            {
                var newWindow = new ArtistMainWindow(artist);
                Application.Current.MainWindow = newWindow;

                newWindow.Show();
                _artistMain.Close();
            }), DispatcherPriority.Background);
        }

        async Task OpenNewWindow(Admin admin)
        {
            await Dispatcher.BeginInvoke(new Action(() =>
            {
                var newWindow = new AdminMainWindow(admin);
                Application.Current.MainWindow = newWindow;

                newWindow.Show();
                _adminMain.Close();
            }), DispatcherPriority.Background);
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
