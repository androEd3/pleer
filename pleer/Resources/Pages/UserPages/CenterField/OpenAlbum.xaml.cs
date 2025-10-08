using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using pleer.Models.CONTEXT;
using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Threading.Tasks;
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

        ListenerMainWindow _listenerMain;

        Listener _listener;

        public OpenAlbum(ListenerMainWindow main, Playlist playlist, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;

            _listener = listener;

            LoadSongsList(playlist);
        }

        async Task LoadPlaylistMetadata(Playlist playlist)
        {
            var listener = _context.Listeners.Find(playlist.CreatorId);

            AlbumName.Text = playlist.Title;
            ArtistName.Text = listener.Name;

            TracksCount.Text = $"Треков: {playlist.SongsId.Count()}";

            //totaliti time
            TimeSpan summaryDuration = TimeSpan.Zero;
            foreach (var song in playlist.Songs)
            {
                summaryDuration += song.TotalDuration;
            }
            SummaryDuration.Text = $"| Длительность: {summaryDuration.ToString(@"mm\:ss")}";

            //creation date
            CreatonDate.Text = playlist.CreationDate.ToString("d MMM yyyy");

            var cover = await _context.AlbumCovers
                .FindAsync(playlist.Id);

            if (cover != null)
                AlbumCoverCenterField.Source = UIServiceMethods.DecodePhoto(cover.FilePath, 200);
            else
            {
                AlbumCoverCenterField.Source = UIServiceMethods.DecodePhoto(InitilizeData.GetDefaultCoverPath(), 200);
            }
        }

        //Create lists
        async Task LoadSongsList(Playlist playlist)
        {
            SongsList.Children.Clear();

            await LoadPlaylistMetadata(playlist);

            var refreshedPlaylist = await _context.Playlists
                .FindAsync(playlist.Id);

            var songs = refreshedPlaylist.SongsId
                .ToList();

            foreach (var id in songs)
            {
                var song = _context.Songs.Find(id);

                var card = UIServiceMethods.CreateSongCard(_listenerMain, _listener, song);
                SongsList.Children.Add(card);
            }
        }
    }
}
