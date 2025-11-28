using pleer.Models.DatabaseContext;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace pleer.Resources.Pages.GeneralPages
{
    /// <summary>
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;
        ArtistMainWindow _artistMain;

        public RegistrationPage(ListenerMainWindow main)
        {
            InitializeComponent();

            _listenerMain = main;
        }

        public RegistrationPage(ArtistMainWindow main)
        {
            InitializeComponent();

            _artistMain = main;
        }

        void UserActiveGrid()
        {
            _listenerMain.FullWindow.Visibility = Visibility.Collapsed;
            _listenerMain.MainGrid.Effect = null;
            _listenerMain.MainGrid.IsEnabled = true;
        }

        void ArtistActiveGrid()
        {
            _artistMain.FullWindow.Visibility = Visibility.Collapsed;
            _artistMain.MainGrid.Effect = null;
            _artistMain.MainGrid.IsEnabled = true;
        }

        private async void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_listenerMain != null)
                {
                    if (!CheckUserDataValid())
                        return;

                    var isEmailValid = ServiceMethods.IsValidEmail(UserEmail.Text);
                    if (!isEmailValid)
                    {
                        ErrorNoticePanel.Text = "Неправильный формат почты";
                        return;
                    }

                    var isPasswordSame = ServiceMethods.IsPasswordsSame(UserPassword.Text, RepeatedUserPassword.Text);
                    if (!isPasswordSame)
                    {
                        ErrorNoticePanel.Text = "Пароли не совпадают";
                        return;
                    }

                    var isPasswordValid = ServiceMethods.IsPasswordsValidOutput(UserPassword.Text);
                    if (isPasswordValid != UserPassword.Text)
                    {
                        ErrorNoticePanel.Text = isPasswordValid;
                        return;
                    }

                    var passwordHash = ServiceMethods.GetSha256Hash(UserPassword.Text);

                    var profilePicture = _context.ProfilePictures
                        .FirstOrDefault(pp => pp.FilePath == InitializeData.GetDefaultProfilePicturePath());

                    var newListener = new Listener()
                    {
                        Name = UserName.Text,
                        Email = UserEmail.Text,
                        ProfilePictureId = profilePicture.Id, // default
                        PasswordHash = passwordHash,
                        CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                    };
                    await _context.Listeners.AddAsync(newListener);
                    await _context.SaveChangesAsync();

                    ServiceMethods.AddPlaylistWithLink(newListener);

                    MessageBox.Show("Вы успешно зарегистрировались", Title = "Регистрация",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    _listenerMain.FullWindow.Navigate(new LoginPage(_listenerMain));
                }

                if (_artistMain != null)
                {
                    if (!CheckUserDataValid())
                        return;

                    var isEmailValid = ServiceMethods.IsValidEmail(UserEmail.Text);
                    if (!isEmailValid)
                    {
                        ErrorNoticePanel.Text = "Неправильный формат почты";
                        return;
                    }

                    var isPasswordSame = ServiceMethods.IsPasswordsSame(UserPassword.Text, RepeatedUserPassword.Text);
                    if (!isPasswordSame)
                    {
                        ErrorNoticePanel.Text = "Пароли не совпадают";
                        return;
                    }

                    var isPasswordValid = ServiceMethods.IsPasswordsValidOutput(UserPassword.Text);
                    if (isPasswordValid != UserPassword.Text)
                    {
                        ErrorNoticePanel.Text = isPasswordValid;
                        return;
                    }

                    var passwordHash = ServiceMethods.GetSha256Hash(UserPassword.Text);

                    var profilePicture = _context.ProfilePictures
                        .FirstOrDefault(pp => pp.FilePath == InitializeData.GetDefaultProfilePicturePath());

                    var newArtist = new Artist()
                    {
                        Name = UserName.Text,
                        Email = UserEmail.Text,
                        ProfilePictureId = profilePicture.Id, // default
                        PasswordHash = passwordHash,
                        CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                    };
                    await _context.Artists.AddAsync(newArtist);
                    await _context.SaveChangesAsync();

                    MessageBox.Show("Вы успешно зарегистрировались", Title = "Регистрация",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    _artistMain.FullWindow.Navigate(new LoginPage(_artistMain));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Во время регистрации произошла ошибка: {ex}", Title = "Регистрация",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        bool CheckUserDataValid()
        {
            if (string.IsNullOrEmpty(UserEmail.Text) ||
                string.IsNullOrEmpty(UserPassword.Text) ||
                string.IsNullOrEmpty(RepeatedUserPassword.Text) ||
                string.IsNullOrEmpty(UserName.Text))
            {
                ErrorNoticePanel.Text = "Заполните все необходимые поля";
                return false;
            }
            else
                return true;
        }

        private void TurnToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            if (_listenerMain != null)
                _listenerMain.FullWindow.Navigate(new LoginPage(_listenerMain));
            if (_artistMain != null)
                _artistMain.FullWindow.Navigate(new LoginPage(_artistMain));
        }

        private void CloseFullWindowFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (_listenerMain != null)
                UserActiveGrid();
            if (_artistMain != null)
                ArtistActiveGrid();
        }

        private void TurnToPassword_Click(object sender, MouseButtonEventArgs e)
        {
            EmailPanel.Visibility = Visibility.Collapsed;
            PasswordPanel.Visibility = Visibility.Visible;
        }

        private void TurnToEmail_Click(object sender, MouseButtonEventArgs e)
        {
            PasswordPanel.Visibility = Visibility.Collapsed;
            EmailPanel.Visibility = Visibility.Visible;
        }
    }
}
