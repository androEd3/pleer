using pleer.Models.DatabaseContext;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace pleer.Resources.Pages.UserPages
{
    /// <summary>
    /// Логика взаимодействия для ArtistProfilePage.xaml
    /// </summary>
    public partial class ArtistProfilePage : Page
    {
        DBContext _context = new();

        ArtistMainWindow _artistMain;
        ListenerMainWindow _listenerMain;

        Artist _artist;
        Listener _listener;

        BitmapImage _picturePath;

        public ArtistProfilePage(ArtistMainWindow artistMain, Artist artist)
        {
            InitializeComponent();

            _artistMain = artistMain;
            _artist = artist;

            LoadArtistsData();
            UserPictureMouseEvents();
        }

        public ArtistProfilePage(ListenerMainWindow listenerMain, Artist artist)
        {
            InitializeComponent();

            _listenerMain = listenerMain;
            _artist = artist;

            Unloaded += ArtistProfilePage_Unloaded;

            LoadArtistsData();
        }

        public ArtistProfilePage(ListenerMainWindow listenerMain, Artist artist, Listener listener)
        {
            InitializeComponent();

            _listenerMain = listenerMain;
            _artist = artist;
            _listener = listener;

            Unloaded += ArtistProfilePage_Unloaded;

            LoadArtistsData();
        }

        private void ArtistProfilePage_Unloaded(object sender, RoutedEventArgs e)
        {
            _listenerMain._currentArtist = null;
        }

        void UserPictureMouseEvents()
        {
            UserPictureGrid.MouseEnter += (s, e) => ChangeProfilePictureGrid.Visibility = Visibility.Visible;
            UserPictureGrid.MouseLeave += (s, e) => ChangeProfilePictureGrid.Visibility = Visibility.Collapsed;
        }


        void LoadArtistsData()
        {
            var artist = _context.Artists
                    .Find(_artist.Id);

            if (artist != null)
            {
                var artistData = _context.Artists
                    .Where(a => a.Id == _artist.Id)
                    .Select(a => new
                    {
                        Artist = a,
                        a.Name,
                        a.ArtistsAlbums,
                })
                .FirstOrDefault();

                int totalAuditions = 0;
                foreach (var album in artistData?.ArtistsAlbums)
                {
                    totalAuditions += album.TotalPlays;
                }

                // metadata
                ArtistName.Text = artistData?.Name;
                TotalPlays.Text = $"{totalAuditions} прослушиваний";

                var picture = _context.ProfilePictures
                        .Find(_artist.ProfilePictureId);

                if (picture != null)
                {
                    UserPicture.ImageSource = UIElementsFactory
                        .DecodePhoto(picture.FilePath, (int)UserPicture.ImageSource.Width);
                }
            }

            // content
            if (_artistMain != null)
                ArtistsContentField.Navigate(new HomePage(_artistMain, _artist));

            if (_listenerMain != null)
            {
                if (_listener != null)
                    ArtistsContentField.Navigate(new HomePage(_listenerMain, _artist, _listener));
                else
                    ArtistsContentField.Navigate(new HomePage(_listenerMain, _artist));
            }
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
                        .DecodePhoto(openFileDialog.FileName, (int)UserPicture.ImageSource.Width);

                    UserPicture.ImageSource = _picturePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        
    }
}
