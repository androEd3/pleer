using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;

namespace pleer.Resources.Pages.Collections
{
    /// <summary>
    /// Логика взаимодействия для OpenAlbum.xaml
    /// </summary>
    public partial class OpenCollection : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;
        ArtistMainWindow _artistMain;

        Album _album;
        Playlist _playlist;

        Listener _listener;

        public OpenCollection(ListenerMainWindow main, Playlist playlist, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;
            _playlist = playlist;

            LoadPlaylistMetadata();
        }

        public OpenCollection(ListenerMainWindow main, Album album, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;
            _album = album;

            LoadAlbumMetadata();
        }

        public OpenCollection(ArtistMainWindow artistMain, Album album)
        {
            InitializeComponent();

            _artistMain = artistMain;
            _album = album;

            LoadAlbumMetadata();
        }

        public OpenCollection()
        {
            InitializeComponent();

            LoadAlbumMetadata();
        }

        void LoadPlaylistMetadata()
        {
            var refreashedPlaylist = _context.Playlists
                .FirstOrDefault(p => p.Id == _playlist.Id);

            var listener = _context.Listeners
                .FirstOrDefault(l => l.Id == _playlist.CreatorId);

            //title & subtitle
            if (refreashedPlaylist != default)
                AlbumName.Text = refreashedPlaylist.Title ?? "Неизвестно";
            if (listener != default)
                ArtistName.Text = listener.Name ?? "Неизвестен";

            //Tracks count
            TracksCount.Text = $"Треков: {refreashedPlaylist.SongsId.Count}";

            //totaliti time
            SummaryDuration.Text = " | Длительность: " + FormattingTotalTime(refreashedPlaylist.SongsId);

            //creation date
            CreatonDate.Text = refreashedPlaylist.CreationDate.ToString("d MMM yyyy");

            //cover
            var cover = _context.PlaylistCovers
               .Find(refreashedPlaylist.CoverId);
            LoadCover(cover.FilePath);

            //Songs list
            LoadPlaylistSongs();
        }

        void LoadAlbumMetadata()
        {
            if (_album == null)
            {
                InformationTextBlock.Visibility = Visibility.Visible;
                AlbumContent.Visibility = Visibility.Collapsed;

                return;
            }

            var refreashedAlbum = _context.Albums
                .FirstOrDefault(p => p.Id == _album.Id);

            var artist = _context.Artists
                .FirstOrDefault(l => l.Id == _album.CreatorId);

            //title & subtitle
            if (refreashedAlbum != default)
                AlbumName.Text = refreashedAlbum.Title ?? "Неизвестно";
            if (artist != default)
                ArtistName.Text = artist.Name ?? "Неизвестен";

            //Tracks count
            TracksCount.Text = $"Треков: {refreashedAlbum.SongsId.Count}";

            //totaliti time
            SummaryDuration.Text = " | Длительность: " + FormattingTotalTime(refreashedAlbum.SongsId);

            //creation date
            CreatonDate.Text = refreashedAlbum.ReleaseDate.ToString("d MMM yyyy");

            //cover
            var cover = _context.AlbumCovers
               .Find(refreashedAlbum.CoverId);
            LoadCover(cover.FilePath);

            //Songs list
            LoadAlbumSongs();
        }

        void LoadCover(string filePath)
        {
            AlbumCoverCenterField.Source = 
                UIElementsFactory
                    .DecodePhoto(filePath, (int)AlbumCoverCenterField.ActualWidth) ??
                UIElementsFactory
                    .DecodePhoto(InitializeData.GetDefaultCoverPath(), (int)AlbumCoverCenterField.ActualWidth);
        }

        string FormattingTotalTime(List<int> collection)
        {
            var summaryDuration = TimeSpan.Zero;

            foreach (var songId in collection)
            {
                var song = _context.Songs.FirstOrDefault(s => s.Id == songId);
                if (song != default) 
                    summaryDuration += song.TotalDuration;
            }

            string formattedDuration;

            if (summaryDuration.TotalHours >= 1)
                formattedDuration = summaryDuration.ToString(@"hh\:mm\:ss");
            else
                formattedDuration = summaryDuration.ToString(@"mm\:ss");

            return formattedDuration;
        }

        //Create lists
        void LoadPlaylistSongs()
        {
            if (_playlist != null)
            {
                var songs = new List<int>();

                var refreshedPlaylist = _context.Playlists.Find(_playlist.Id);
                if (refreshedPlaylist != null)
                    songs = refreshedPlaylist.SongsId
                        .ToList();

                foreach (var id in songs)
                {
                    var song = _context.Songs.Find(id);
                        
                    if (song != null)
                    {
                        var card = UIElementsFactory.CreateSongCard(song, _listener, _listenerMain.SongCard_Click);

                        SongsList.Children.Add(card);
                    }
                }
            }
        }

        void LoadAlbumSongs()
        {
            if (_album != null)
            {
                var songs = new List<int>();

                var refreshedAlbum = _context.Albums.Find(_album.Id);
                if (refreshedAlbum != null)
                    songs = refreshedAlbum.SongsId.ToList();

                foreach (var id in songs)
                {
                    var song = _context.Songs.Find(id);

                    if (song != null)
                    {
                        var card = new UIElement();

                        if (_artistMain != null)
                            card = UIElementsFactory.CreateSongCard(song, _artistMain.SongCard_Click);
                        else if (_listenerMain != null)
                            card = UIElementsFactory.CreateSongCard(song, _listener, _listenerMain.SongCard_Click);

                        SongsList.Children.Add(card);
                    }
                }
            }
        }
    }
}
