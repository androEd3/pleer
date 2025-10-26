using pleer.Models.CONTEXT;
using pleer.Models.DB_Models;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace pleer.Resources.Pages.GeneralPages
{
    /// <summary>
    /// Логика взаимодействия для ChangePasswordPage.xaml
    /// </summary>
    public partial class ChangePasswordPage : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;
        ArtistMainWindow _artistMain;

        Listener _listener;
        Artist _artist;

        public ChangePasswordPage(ListenerMainWindow main, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;
        }

        public ChangePasswordPage(ArtistMainWindow main, Artist artist)
        {
            InitializeComponent();

            _artistMain = main;
            _artist = artist;
        }

        void UserActiveGrid()
        {
            _listenerMain.FullWindow.Visibility = Visibility.Collapsed;
            _listenerMain.WindowBlurEffect.Radius = 0;
            _listenerMain.MainGrid.IsEnabled = true;
        }

        void ArtistActiveGrid()
        {
            _artistMain.FullWindow.Visibility = Visibility.Collapsed;
            _artistMain.WindowBlurEffect.Radius = 0;
            _artistMain.MainGrid.IsEnabled = true;
        }

        private async void SaveUserDataButton_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = ServiceMethods.IsPasswordsValidOutput(OldUserPassword.Text);
            if (oldPassword != OldUserPassword.Text)
            {
                ErrorNoticePanel.Style = Application.Current.TryFindResource("SmallErrorPanel") as Style;
                ErrorNoticePanel.Text = oldPassword;
                return;
            }

            string isPasswordsSame = ServiceMethods.IsPasswordsSame(NewUserPassword.Text, RepeatedNewUserPassword.Text);
            if (isPasswordsSame != NewUserPassword.Text)
            {
                ErrorNoticePanel.Style = Application.Current.TryFindResource("SmallErrorPanel") as Style;
                ErrorNoticePanel.Text = isPasswordsSame;
                return;
            }

            string newPassword = ServiceMethods.IsPasswordsValidOutput(NewUserPassword.Text);
            if (newPassword != NewUserPassword.Text)
            {
                ErrorNoticePanel.Style = Application.Current.TryFindResource("SmallErrorPanel") as Style;
                ErrorNoticePanel.Text = newPassword;
                return;
            }

            try
            {
                var artist = await _context.Artists
                    .FindAsync(_artist.Id);
                if (artist != null)
                {
                    string oldPasswordHash = ServiceMethods.GetSha256Hash(oldPassword);
                    if (oldPasswordHash != artist.PasswordHash)
                    {
                        ErrorNoticePanel.Style = Application.Current.TryFindResource("SmallErrorPanel") as Style;
                        ErrorNoticePanel.Text = "Неверный старый пароль";
                        return;
                    }
                    ChangeArtistPassword(artist, newPassword);
                }

                var listener = await _context.Listeners
                    .FindAsync(_listener.Id);
                if (listener != null)
                {
                    string oldPasswordHash = ServiceMethods.GetSha256Hash(oldPassword);
                    if (oldPasswordHash != listener.PasswordHash)
                    {
                        ErrorNoticePanel.Style = Application.Current.TryFindResource("SmallErrorPanel") as Style;
                        ErrorNoticePanel.Text = "Неверный старый пароль";
                        return;
                    }
                    ChangeListenerPassword(listener, newPassword);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка смены пароля: {ex}", Title = "Профиль",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        async void ChangeArtistPassword(Artist artist, string newPassword)
        {
            string newPasswordHash = ServiceMethods.GetSha256Hash(newPassword);

            artist.PasswordHash = newPasswordHash;
            await _context.SaveChangesAsync();

            MessageBox.Show("Пароль успешно изменен", Title = "Профиль",
                                MessageBoxButton.OK, MessageBoxImage.Information);

            NavigateMethods.OpenArtistProfile(_artistMain, _artist);
        }

        async void ChangeListenerPassword(Listener listener, string newPassword)
        {
            string newPasswordHash = ServiceMethods.GetSha256Hash(newPassword);

            listener.PasswordHash = newPasswordHash;
            await _context.SaveChangesAsync();

            MessageBox.Show("Пароль успешно изменен", Title = "Профиль",
                                MessageBoxButton.OK, MessageBoxImage.Information);

            NavigateMethods.OpenListenerProfile(_listenerMain, _listener);
        }

        private void TurnToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            if (_listenerMain != null)
                NavigateMethods.OpenListenerProfile(_listenerMain, _listener);
            if (_artistMain != null)
                NavigateMethods.OpenArtistProfile(_artistMain, _artist);
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
