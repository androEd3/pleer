using pleer.Models.CONTEXT;
using pleer.Models.DB_Models;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Pages.AlbumsAndPlaylists;
using pleer.Resources.Pages.GeneralPages;
using System.Windows;

namespace pleer.Resources.Windows
{
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

        void LoadArtistData()
        {
            InitializeData.SeedData(_context);

            if (_artist != null)
            {
                LoginButton.Visibility = Visibility.Collapsed;
                ProfilePictureEllipse.Visibility = Visibility.Visible;

                ArtistName.Text = _artist.Name;

                var picture = _context.ProfilePictures
                    .Find(_artist.ProfilePictureId);

                if (picture != null)
                    ProfilePictureImage.ImageSource = UIElementsFactory
                        .DecodePhoto(picture.FilePath, 100);
                else
                    ProfilePictureImage.ImageSource = UIElementsFactory
                        .DecodePhoto(InitializeData.GetDefaultProfilePicturePath(), 100);
            }
            else
            {
                ArtistFunctionsList.IsEnabled = false;
                OperationField.Navigate(new UnauthorizedNoticePage(this));
            }
        }

        private void ArtistReleasesButton_Click(object sender, RoutedEventArgs e)
        {
            OperationField.Navigate(new AlbumsList(this, _artist));
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

        private void LoadAlbumButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ArtistProfileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CooperationButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
