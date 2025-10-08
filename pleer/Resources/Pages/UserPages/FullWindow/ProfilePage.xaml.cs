using Microsoft.EntityFrameworkCore;
using NAudio.MediaFoundation;
using pleer.Models.CONTEXT;
using pleer.Models.DB_Models;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace pleer.Resources.Pages.UserPages.FullWindow
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        DBContext _context = new();

        PictureService _pictureService = new();

        ListenerMainWindow _listenerMain;
        ArtistMainWindow _artistMain;

        Listener _listener;
        Artist _artist;

        BitmapImage _picturePath;

        public ProfilePage(ListenerMainWindow main, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;

            UserInactiveGrid();
        }

        public ProfilePage(ArtistMainWindow main, Artist artist)
        {
            InitializeComponent();

            _artistMain = main;
            _artist = artist;

            ArtistInactiveGrid();
        }

        async Task LoadListenerData()
        {
            var listener = await _context.Listeners
                    .FindAsync(_listener.Id);

            if (listener != null)
            {
                UserName.Text = listener.Name;
                UserEmail.Text = listener.Email;

                var picture = await _context.ProfilePictures
                    .FindAsync(listener.ProfilePictureId);

                if (picture != null)
                    UserPicture.ImageSource = UIServiceMethods
                        .DecodePhoto(picture.FilePath, 175);

                _artistMain.ProfilePictureImage.ImageSource = UIServiceMethods
                        .DecodePhoto(picture.FilePath, 175);
            }
        }

        async Task LoadArtistData()
        {
            var artist = await _context.Artists
                    .FindAsync(_artist.Id);

            if (artist != null)
            {
                UserName.Text = artist.Name;
                UserEmail.Text = artist.Email;

                var picture = await _context.ProfilePictures
                    .FindAsync(artist.ProfilePictureId);

                if (picture != null)
                    UserPicture.ImageSource = UIServiceMethods
                        .DecodePhoto(picture.FilePath, 175);

                _artistMain.ProfilePictureImage.ImageSource = UIServiceMethods
                        .DecodePhoto(picture.FilePath, 175);
            }
        }

        void UserPictureMouseEvents()
        {
            UserPictureGrid.MouseEnter += (s, e) => ChangeProfilePictureGrid.Visibility = Visibility.Visible;
            UserPictureGrid.MouseLeave += (s, e) => ChangeProfilePictureGrid.Visibility = Visibility.Collapsed;
        }

        void UserInactiveGrid()
        {
            UserPictureMouseEvents(); LoadListenerData();

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
            UserPictureMouseEvents(); LoadArtistData();

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

        private async void SaveUserDataButton_Click(object sender, RoutedEventArgs e)
        {
            var email = ServiceMethods.IsValidEmailOutput(UserEmail.Text);
            if (email != UserEmail.Text)
            {
                ErrorNoticePanel.Style = Application.Current.TryFindResource("SmallErrorPanel") as Style;
                ErrorNoticePanel.Text = email;
                return;
            }

            if (string.IsNullOrEmpty(UserName.Text))
            {
                ErrorNoticePanel.Style = Application.Current.TryFindResource("SmallErrorPanel") as Style;
                ErrorNoticePanel.Text = "Имя пользователя не может быть пустым";
                return;
            }

            try
            {
                if (_artist != null)
                    UpdateArtistData(email);
                if (_listener != null)
                    UpdateListenerData(email);
            }
            catch
            {
                ErrorNoticePanel.Style = Application.Current.TryFindResource("SmallErrorPanel") as Style;
                ErrorNoticePanel.Text = "Ошибка сервера";
            }
        }

        async void UpdateArtistData(string email)
        {
            var artist = await _context.Artists
                    .FindAsync(_artist.Id);

            if (artist != null)
            {
                artist.Name = UserName.Text;
                artist.Email = email;

                await _context.SaveChangesAsync();

                if (_picturePath != null)
                {
                    var profilePicture = new ProfilePicture
                    {
                        FilePath = _picturePath.ToString()
                    };

                    var existProfilePicture = await _context.ProfilePictures
                        .FirstOrDefaultAsync(pp => pp.FilePath == profilePicture.FilePath);

                    if (existProfilePicture == null)
                    {
                        var newPicture = _pictureService.SaveProfilePicture(_picturePath.ToString(), _artist, _listener);
                        await _context.AddAsync(newPicture);
                        await _context.SaveChangesAsync();

                        _artist.ProfilePictureId = newPicture.Id;
                    }
                    else
                    {
                        _artist.ProfilePictureId = existProfilePicture.Id;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            await LoadArtistData();

            ErrorNoticePanel.Style = Application.Current.TryFindResource("SmallSuccessPanel") as Style;
            ErrorNoticePanel.Text = "Изменения успешно сохранены!";
        }

        async void UpdateListenerData(string email)
        {
            var listener = await _context.Listeners
                    .FindAsync(_listener.Id);

            if (listener != null)
            {
                listener.Name = UserName.Text;
                listener.Email = email;

                await _context.SaveChangesAsync();

                if (_picturePath != null)
                {
                    var profilePicture = new ProfilePicture
                    {
                        FilePath = _picturePath.ToString()
                    };

                    var existProfilePicture = await _context.ProfilePictures
                        .FirstOrDefaultAsync(pp => pp.FilePath == profilePicture.FilePath);

                    if (existProfilePicture == null)
                    {
                        var newPicture = _pictureService.SaveProfilePicture(_picturePath.ToString(), _artist, _listener);
                        await _context.AddAsync(newPicture);
                        await _context.SaveChangesAsync();

                        listener.ProfilePictureId = newPicture.Id;
                    }
                    else
                    {
                        listener.ProfilePictureId = existProfilePicture.Id;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            await LoadListenerData();

            ErrorNoticePanel.Style = Application.Current.TryFindResource("SmallSuccessPanel") as Style;
            ErrorNoticePanel.Text = "Изменения успешно сохранены!";
        }

        private void CloseFullWindowFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (_listenerMain != null)
                UserActiveGrid();
            if (_artistMain != null)
                ArtistActiveGrid();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_listenerMain != null)
                NavigateMethods.OpenListenerChangePasswordPage(_listenerMain, _listener);
            if (_artistMain != null)
                NavigateMethods.OpenArtistChangePasswordPage(_artistMain, _artist);
        }

        private void ChangeProfilePicture_Click(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выберите изображение профиля",
                Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _picturePath = UIServiceMethods
                        .DecodePhoto(openFileDialog.FileName, 200);

                    UserPicture.ImageSource = _picturePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void Logout_Click(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.BeginInvoke(new Action(() =>
            {
                var newWindow = new ArtistMainWindow();
                Application.Current.MainWindow = newWindow;

                _artistMain.Close();
                newWindow.Show();
            }), DispatcherPriority.Background);
        }
    }
}
