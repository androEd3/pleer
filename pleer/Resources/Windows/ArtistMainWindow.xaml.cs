using pleer.Models.CONTEXT;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using System.Windows;

namespace pleer.Resources.Windows
{
    /// <summary>
    /// Логика взаимодействия для ArtistMainWindow.xaml
    /// </summary>
    public partial class ArtistMainWindow : Window
    {
        DBContext _context = new();

        Artist _artist;

        public ArtistMainWindow()
        {
            InitializeComponent();

            LoadArtistData();
        }

        public ArtistMainWindow(Artist artist)
        {
            InitializeComponent();

            _artist = artist;

            LoadArtistData();
        }

        async Task LoadArtistData()
        {
            InitilizeData.SeedData(_context);

            if (_artist != null)
            {
                LoginButton.Visibility = Visibility.Collapsed;
                ProfilePictureEllipse.Visibility = Visibility.Visible;

                var picture = await _context.ProfilePictures
                    .FindAsync(_artist.ProfilePictureId);

                if (picture != null)
                    ProfilePictureImage.ImageSource = UIServiceMethods
                        .DecodePhoto(picture.FilePath, 100);
                else
                {
                    ProfilePictureImage.ImageSource = UIServiceMethods
                        .DecodePhoto(InitilizeData.GetDefaultProfilePicturePath(), 100);
                }
            }
        }

        private void LoadAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateMethods.CreateAlbum(this, _artist);
        }

        private void LoginAsListinerButton_Click(object sender, RoutedEventArgs e)
        {
            new ListenerMainWindow().Show(); this.Close();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateMethods.OpenArtistLoginPage(this);
        }

        private void ProfileImage_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigateMethods.OpenArtistProfile(this, _artist);
        }
    }
}
