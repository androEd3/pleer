using pleer.Models.CONTEXT;
using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace pleer.Resources.Pages
{
    /// <summary>
    /// Логика взаимодействия для OpenAlbum.xaml
    /// </summary>
    public partial class OpenAlbum : Page
    {
        DBContext _context = new();

        UserMainWindow _userMainWindow;

        User _user;

        public OpenAlbum(UserMainWindow main, Playlist playlist, User user)
        {
            InitializeComponent();

            _userMainWindow = main;

            _user = user;

            LoadSongsList(playlist);
        }

        void LoadPlaylistMetadata(Playlist playlist)
        {
            var user = _context.Users.Find(playlist.CreatorId);

            AlbumName.Text = playlist.Title;
            ArtistName.Text = user.Name;

            TracksCount.Text = $"Треков: {playlist.SongsId.Count()}";

            //TimeSpan summaryDuration = TimeSpan.Zero;
            //foreach (var song in playlist.Songs)
            //{
            //    summaryDuration += song.DurationSeconds;
            //}
            //SummaryDuration.Text = $"| Длительность: {summaryDuration.ToString(@"mm\:ss")}";

            CreatonDate.Text = playlist.CreationDate.ToString("d MMM yyyy");

            var cover = _context.AlbumCovers.Find(playlist.AlbumCoverId);

            if (string.IsNullOrEmpty(cover.FilePath))
                AlbumCoverCenterField.Source = new BitmapImage(new Uri("..\\Resources\\ServiceImages\\NoMediaImage.png"));
            else
                AlbumCoverCenterField.Source = UIServiceMethods.DecodePhoto(cover.FilePath, 90);
        }

        //Create lists
        void LoadSongsList(Playlist playlist)
        {
            SongsList.Children.Clear();

            LoadPlaylistMetadata(playlist);

            var refreshedPlaylist = _context.Playlists.Find(playlist.Id);

            var songs = refreshedPlaylist.SongsId
                .ToList();

            foreach (var id in songs)
            {
                var song = _context.Songs.Find(id);

                var card = UIServiceMethods.CreateSongCard(_userMainWindow, _user, song);
                SongsList.Children.Add(card);
            }
        }
    }
}
