using pleer.Models.CONTEXT;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Pages;
using System.Diagnostics;
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

            LoadArtistData(_artist);
        }

        public ArtistMainWindow(Artist artist)
        {
            InitializeComponent();

            _artist = artist;

            LoadArtistData(_artist);
        }

        void LoadArtistData(Artist artist)
        {
            InitilizeData.SeedData();

            if (artist != null)
            {
                MessageBox.Show(artist.Name);
            }
        }

        private void LoadAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateMethods.CreateAlbum(this, _artist);
        }

        private void LoginAsListinerButton_Click(object sender, RoutedEventArgs e)
        {
            new UserMainWindow().Show(); this.Close();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateMethods.OpenArtistLoginPage(this);
        }

        void OpenLoginBrowser()
        {
            var authUrl = "https://localhost:7021/Home/Index";
            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });
        }
    }
}
