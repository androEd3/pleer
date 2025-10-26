using pleer.Models.CONTEXT;
using pleer.Models.DB_Models;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace pleer.Resources.Pages.GeneralPages
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

        string _email;

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

        void LoadListenerData()
        {
            var listener = _context.Listeners
                    .Find(_listener.Id);

            if (listener != null)
            {
                UserName.Text = listener.Name;
                UserEmail.Text = listener.Email;

                var picture = _context.ProfilePictures
                    .Find(listener.ProfilePictureId);

                if (picture != null)
                {
                    UserPicture.ImageSource = UIElementsFactory
                        .DecodePhoto(picture.FilePath, 80);
                    _listenerMain.ProfilePictureImage.ImageSource = UIElementsFactory
                        .DecodePhoto(picture.FilePath, 80);
                }
            }
        }

        void LoadArtistData()
        {
            var artist = _context.Artists
                    .Find(_artist.Id);

            if (artist != null)
            {
                UserName.Text = artist.Name;
                UserEmail.Text = artist.Email;

                var picture = _context.ProfilePictures
                    .Find(artist.ProfilePictureId);

                if (picture != null)
                {
                    UserPicture.ImageSource = UIElementsFactory
                        .DecodePhoto(picture.FilePath, 175);
                    _artistMain.ProfilePictureImage.ImageSource = UIElementsFactory
                        .DecodePhoto(picture.FilePath, 175);
                }
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

        bool IsValidCheck()
        {
            _email = ServiceMethods.IsValidEmailOutput(UserEmail.Text);

            if (_email != UserEmail.Text)
            {
                ErrorPanelContent(_email);
                return false;
            }
            if (string.IsNullOrEmpty(UserName.Text))
            {
                ErrorPanelContent("Имя пользователя не может быть пустым");
                return false;
            }

            return true;
        }

        async Task UpdateUserData()
        {
            try
            {
                if (_artist != null)
                    await UpdateArtistData(_email);
                if (_listener != null)
                    await UpdateListenerData(_email);
            }
            catch
            {
                ErrorPanelContent("Ошибка сервера!");
            }
        }

        private async void SaveUserDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidCheck())
                return;

            await UpdateUserData();
        }

        void ErrorPanelContent(string errorMessage)
        {
            ErrorNoticePanel.Style = Application.Current.TryFindResource("SmallErrorPanel") as Style;
            ErrorNoticePanel.Text = errorMessage;
        }
        void SuccessPanelContent(string errorMessage)
        {
            ErrorNoticePanel.Style = Application.Current.TryFindResource("SmallErrorPanel") as Style;
            ErrorNoticePanel.Text = errorMessage;
        }

        async Task UpdateArtistData(string email)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var artist = await _context.Artists
                    .FindAsync(_artist.Id);

                if (artist != null)
                {
                    artist.Name = UserName.Text;
                    artist.Email = email;

                    if (_picturePath != null)
                    {
                        var newPicture = _pictureService
                            .SaveProfilePicture(_picturePath.ToString(), _artist, _listener);
                        _context.Add(newPicture);
                        await _context.SaveChangesAsync();

                        artist.ProfilePictureId = newPicture.Id;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                LoadArtistData();

                SuccessPanelContent("Изменения успешно сохранены!");
            }
            catch
            {
                SuccessPanelContent("Ошмбка сервера!");
            }
        }

        async Task UpdateListenerData(string email)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var listener = await _context.Artists
                    .FindAsync(_artist.Id);

                if (listener != null)
                {
                    listener.Name = UserName.Text;
                    listener.Email = email;

                    if (_picturePath != null)
                    {
                        var newPicture = _pictureService
                            .SaveProfilePicture(_picturePath.ToString(), _artist, _listener);
                        _context.Add(newPicture);
                        await _context.SaveChangesAsync();

                        listener.ProfilePictureId = newPicture.Id;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                LoadListenerData();

                SuccessPanelContent("Изменения успешно сохранены!");
            }
            catch
            {
                SuccessPanelContent("Ошмбка сервера!");
            }
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
                _listenerMain.FullWindow.Navigate(new ChangePasswordPage(_listenerMain, _listener));
            if (_artistMain != null)
                _artistMain.FullWindow.Navigate(new ChangePasswordPage(_artistMain, _artist));
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
                    _picturePath = UIElementsFactory
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
            await Logout();
        }

        async Task Logout()
        {
            if (_artist != null)
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    var newWindow = new ArtistMainWindow();
                    Application.Current.MainWindow = newWindow;

                    newWindow.Show();
                    _artistMain.Close();
                }), DispatcherPriority.Background);
            }

            if (_listener != null)
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    var newWindow = new ListenerMainWindow();
                    Application.Current.MainWindow = newWindow;

                    newWindow.Show();
                    _listenerMain.Close();
                }), DispatcherPriority.Background);
            }
        }
    }
}
