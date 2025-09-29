using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.ApplicationServices;
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
using System.Security.Cryptography;
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
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace pleer.Resources.Pages.UserPages.FullWindow
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        DBContext _context = new();

        UserMainWindow _userMain;
        ArtistMainWindow _artistMain;

        MailAddress _email;

        public LoginPage(UserMainWindow main)
        {
            InitializeComponent();

            _userMain = main;

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
            _userMain.FullWindow.Visibility = Visibility.Visible;
            _userMain.WindowBlurEffect.Radius = 15;
            _userMain.MainGrid.IsEnabled = false;
        }

        void UserActiveGrid()
        {
            _userMain.FullWindow.Visibility = Visibility.Collapsed;
            _userMain.WindowBlurEffect.Radius = 0;
            _userMain.MainGrid.IsEnabled = true;
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

            if (!IsValidEmail())
                return;

            if (!IsPasswordsValid())
                return;

            try
            {
                if (_userMain != null)
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == _email.Address);

                    if (user == default)
                    {
                        ErrorNoticePanel.Text = "Пользователь не найден";
                        return;
                    }

                    var pass = ServiceMethods.GetSha256Hash(UserPassword.Text);

                    if (user.PasswordHash != pass)
                    {
                        ErrorNoticePanel.Text = "Неверный пароль";
                        return;
                    }

                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var newWindow = new UserMainWindow(user);
                        Application.Current.MainWindow = newWindow;

                        _userMain.Close();
                        newWindow.Show();
                    }), DispatcherPriority.Background);
                }

                if (_artistMain != null)
                {
                    var artist = await _context.Artists
                        .FirstOrDefaultAsync(u => u.Email == _email.Address);

                    if (artist == default)
                    {
                        ErrorNoticePanel.Text = "Исполнитель не найден";
                        return;
                    }

                    var pass = ServiceMethods.GetSha256Hash(UserPassword.Text);

                    if (artist.PasswordHash != pass)
                    {
                        ErrorNoticePanel.Text = "Неверный пароль";
                        return;
                    }

                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var newWindow = new ArtistMainWindow(artist);
                        Application.Current.MainWindow = newWindow;

                        _artistMain.Close();
                        newWindow.Show();
                    }), DispatcherPriority.Background);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex}");
            }
        }

        bool IsValidEmail()
        {
            string email = UserEmail.Text;

            var trimmedEmail = email.Trim();

            if (!trimmedEmail.EndsWith("."))
            {
                try
                {
                    _email = new MailAddress(email);
                    return _email.Address == trimmedEmail;
                }
                catch
                {
                    ErrorNoticePanel.Text = "Неправильный формат почты";
                    return false;
                }
            }
            return false;
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

        bool IsPasswordsValid()
        {
            string password1 = UserPassword.Text;

            if (password1.Length < 6)
            {
                ErrorNoticePanel.Text = "Пароль должен содержать минимум 6 символов";
                return false;
            }

            if (password1.Length > 32)
            {
                ErrorNoticePanel.Text = "Пароль не должен превышать 32 символа";
                return false;
            }

            bool hasDigit = password1.Any(char.IsDigit);
            bool hasLetter = password1.Any(char.IsLetter);

            if (!hasDigit)
            {
                ErrorNoticePanel.Text = "Пароль должен содержать хотя бы одну цифру";
                return false;
            }

            if (!hasLetter)
            {
                ErrorNoticePanel.Text = "Пароль должен содержать хотя бы одну букву";
                return false;
            }

            return true;
        }

        private void TurnToRegistration_Click(object sender, MouseButtonEventArgs e)
        {
            if (_userMain != null)
                NavigateMethods.OpenUserRegistrationPage(_userMain);
            if (_artistMain != null)
                NavigateMethods.OpenArtistRegistrationPage(_artistMain);
        }

        private void CloseFullWindowFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (_userMain != null)
                UserActiveGrid();
            if (_artistMain != null)
                ArtistActiveGrid();
        }
    }
}
