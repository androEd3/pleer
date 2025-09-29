using pleer.Models.CONTEXT;
using pleer.Models.DB_Models;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.Mail;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace pleer.Resources.Pages.UserPages.FullWindow
{
    /// <summary>
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        DBContext _context = new();

        UserMainWindow _userMain;
        ArtistMainWindow _artistMain;

        public RegistrationPage(UserMainWindow main)
        {
            InitializeComponent();

            _userMain = main;
        }

        public RegistrationPage(ArtistMainWindow main)
        {
            InitializeComponent();

            _artistMain = main;
        }

        void UserActiveGrid()
        {
            _userMain.FullWindow.Visibility = Visibility.Collapsed;
            _userMain.WindowBlurEffect.Radius = 0;
            _userMain.MainGrid.IsEnabled = true;
        }

        void ArtistActiveGrid()
        {
            _artistMain.FullWindow.Visibility = Visibility.Collapsed;
            _artistMain.WindowBlurEffect.Radius = 0;
            _artistMain.MainGrid.IsEnabled = true;
        }

        private async void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_userMain != null)
                {
                    if (!CheckUserDataValid())
                        return;

                    string isEmailValid = ServiceMethods.IsValidEmailOutput(UserEmail.Text);
                    if (string.IsNullOrEmpty(isEmailValid))
                    {
                        ErrorNoticePanel.Text = "Неправильный формат почты";
                        return;
                    }

                    string isPasswordValid = ServiceMethods.IsPasswordsValidOutput(UserPassword.Text, RepeatedUserPassword.Text);
                    if (!string.IsNullOrEmpty(isPasswordValid))
                    {
                        ErrorNoticePanel.Text = isPasswordValid;
                        return;
                    }

                    var password = ServiceMethods.GetSha256Hash(UserPassword.Text);

                    var newUser = new User()
                    {
                        Name = UserName.Text,
                        Email = UserEmail.Text,
                        PasswordHash = password,
                        CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                    };
                    await _context.Users.AddAsync(newUser);
                    await _context.SaveChangesAsync();

                    ServiceMethods.AddPlaylistWithLink(newUser, _context, true);

                    MessageBox.Show("Вы успешно зарегистрировались", Title = "Регистрация",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    NavigateMethods.OpenUserLoginPage(_userMain);
                }

                if (_artistMain != null)
                {
                    if (!CheckUserDataValid())
                        return;

                    string isEmailValid = ServiceMethods.IsValidEmailOutput(UserEmail.Text);
                    if (string.IsNullOrEmpty(isEmailValid))
                    {
                        ErrorNoticePanel.Text = "Неправильный формат почты";
                        return;
                    }

                    string isPasswordValid = ServiceMethods.IsPasswordsValidOutput(UserPassword.Text, RepeatedUserPassword.Text);
                    if (!string.IsNullOrEmpty(isPasswordValid))
                    {
                        ErrorNoticePanel.Text = isPasswordValid;
                        return;
                    }

                    var password = ServiceMethods.GetSha256Hash(UserPassword.Text);

                    var newArtist = new Artist()
                    {
                        Name = UserName.Text,
                        Email = UserEmail.Text,
                        PasswordHash = password,
                        CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                    };
                    await _context.Artists.AddAsync(newArtist);
                    await _context.SaveChangesAsync();

                    MessageBox.Show("Вы успешно зарегистрировались", Title = "Регистрация",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    NavigateMethods.OpenArtistLoginPage(_artistMain);
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
            if (_userMain != null)
                NavigateMethods.OpenUserLoginPage(_userMain);
            if (_artistMain != null)
                NavigateMethods.OpenArtistLoginPage(_artistMain);
        }

        private void CloseFullWindowFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (_userMain != null)
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
