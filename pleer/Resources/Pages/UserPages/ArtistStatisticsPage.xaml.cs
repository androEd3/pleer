using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Users;
using System.Windows;
using System.Windows.Controls;

namespace pleer.Resources.Pages.UserPages
{
    /// <summary>
    /// Логика взаимодействия для ArtistStatisticsPage.xaml
    /// </summary>
    public partial class ArtistStatisticsPage : Page
    {
        DBContext _context = new();

        Artist _artist;

        public ArtistStatisticsPage(Artist artist)
        {
            InitializeComponent();

            _artist = artist;

            LoadStatistics();
        }

        void LoadStatistics()
        {
            var artistData = _context.Artists
                .Where(a => a.Id == _artist.Id)
                .Select(a => new
                {
                    Artist = a,
                    a.Name,
                    a.ArtistsAlbums,
                    a.CreatedAt,
                })
                .FirstOrDefault();

            ArtistName.Text = $"Статистика исполнителя {artistData?.Name}";
            AccountCreatedAt.Text = $"{artistData?.CreatedAt}";

            int totalAuditions = 0;
            foreach (var album in artistData?.ArtistsAlbums)
            {
                totalAuditions += album.TotalPlays;
            }
            TotalAuditions.Text = totalAuditions.ToString();

            TotalAlbums.Text = artistData?.ArtistsAlbums.Count.ToString();

            int totalSongs = 0;
            foreach (var album in artistData?.ArtistsAlbums)
            {
                totalSongs += album.SongsId.Count;
            }
            TotalSongs.Text = totalSongs.ToString();

            List<Song> artistsSong = [];
            foreach (var album in artistData?.ArtistsAlbums)
            {
                var albumData = _context.Albums
                    .Where(a => a.Id == album.Id)
                    .Select(a => new
                    {
                        Album = a,
                        a.Songs
                    })
                    .FirstOrDefault();

                artistsSong.AddRange(albumData?.Songs);
            }
            var mostPopularSong = artistsSong
                .OrderByDescending(s => s.TotalPlays)
                .First();
            MostPopularSong.Text = mostPopularSong.Title + $" ({mostPopularSong.TotalPlays} прослушиваний)";

            var mostPopularAlbum = artistData?.ArtistsAlbums
                .OrderByDescending(s => s.TotalPlays)
                .First();
            MostPopularAlbum.Text = mostPopularAlbum.Title + $" ({mostPopularAlbum.TotalPlays} прослушиваний)";
        }

        private void UpdateArtistDataButton_Click(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
        }
    }
}
