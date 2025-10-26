using pleer.Models.CONTEXT;
using pleer.Models.DB_Models;
using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows.Controls;
using System.Windows;

namespace pleer.Resources.Pages.AlbumsAndPlaylists
{
    /// <summary>
    /// Логика взаимодействия для OpenAlbum.xaml
    /// </summary>
    public partial class OpenAlbum : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;

        Album _album;
        Playlist _playlist;

        Listener _listener;
        Artist _artist;

        public OpenAlbum(ListenerMainWindow main, Playlist playlist, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;
            _playlist = playlist;

            LoadPlaylistMetadata();
        }

        public OpenAlbum(ListenerMainWindow main, Album album, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;
            _album = album;

            LoadAlbumMetadata();
        }

        public OpenAlbum(Album album, Artist artist)
        {
            InitializeComponent();

            _album = album;
            _artist = artist;

            LoadAlbumMetadata();
        }

        public OpenAlbum()
        {
            InitializeComponent();

            LoadAlbumMetadata();
        }

        void LoadPlaylistMetadata()
        {
            var listener = _context.Listeners
                .Find(_playlist.CreatorId);

            if (listener != null)
            {
                AlbumName.Text = _playlist.Title;
                ArtistName.Text = listener.Name;
            }

            //Tracks count
            TracksCount.Text = $"Треков: {_playlist.SongsId.Count()}";

            //totaliti time
            TimeSpan summaryDuration = TimeSpan.Zero;
            foreach (var song in _playlist.Songs)
            {
                summaryDuration += song.TotalDuration;
            }
            SummaryDuration.Text = $" | Длительность: {summaryDuration.ToString(@"mm\:ss")}";

            //creation date
            CreatonDate.Text = _playlist.CreationDate.ToString("d MMM yyyy");

            //cover
            var cover = _context.PlaylistCovers
                .Find(_playlist.CoverId);

            if (cover != null)
                AlbumCoverCenterField.Source = UIElementsFactory
                    .DecodePhoto(cover.FilePath, 200);
            else
                AlbumCoverCenterField.Source = UIElementsFactory
                    .DecodePhoto(InitializeData.GetDefaultCoverPath(), 200);

            LoadSongListForListener();
        }

        void LoadAlbumMetadata()
        {
            if (_album == null)
            {
                InformationTextBlock.Visibility = Visibility.Visible;
                AlbumContent.Visibility = Visibility.Collapsed;

                return;
            }

            if (_artist != null)
            {
                AlbumName.Text = _album.Title;
                ArtistName.Text = _artist.Name;
            }
            else
            {
                AlbumName.Text = "Неизвестно";
                ArtistName.Text = "Неизвестен";
            }

            //Tracks count
            TracksCount.Text = $"Треков: {_album.SongsId.Count()}";

            //totaliti time
            TimeSpan summaryDuration = TimeSpan.Zero;
            foreach (var Id in _album.SongsId)
            {
                var song = _context.Songs.Find(Id);
                if (song != null)
                    summaryDuration += song.TotalDuration;
            }
            SummaryDuration.Text = $" | Длительность: {summaryDuration.ToString(@"mm\:ss")}";

            //creation date
            CreatonDate.Text = _album.ReleaseDate.ToString("d MMM yyyy");

            //cover
            var cover = _context.AlbumCovers
                .Find(_album.CoverId);

            if (cover != null)
                AlbumCoverCenterField.Source = UIElementsFactory
                    .DecodePhoto(cover.FilePath, 200);
            else
                AlbumCoverCenterField.Source = UIElementsFactory
                    .DecodePhoto(InitializeData.GetDefaultCoverPath(), 200);

            LoadSongsListForArtist();
        }

        //Create lists
        void LoadSongListForListener()
        {
            SongsList.Children.Clear();

            if (_album != null)
            {
                var songs = _album.SongsId
                    .ToList();

                foreach (var id in songs)
                {
                    var song = _context.Songs
                        .Find(id);

                    if (song != null)
                    {
                        var card = UIElementsFactory
                            .CreateSongCardListener(_listenerMain, _listener, song);
                        SongsList.Children.Add(card);
                    }
                }
            }

            if (_playlist != null)
            {
                var songs = _playlist.SongsId
                    .ToList();

                foreach (var id in songs)
                {
                    var song = _context.Songs
                        .Find(id);

                    if (song != null)
                    {
                        var card = UIElementsFactory
                            .CreateSongCardListener(_listenerMain, _listener, song);
                        SongsList.Children.Add(card);
                    }
                }
            }
        }

        void LoadSongsListForArtist()
        {
            if (_album != null)
            {
                var songs = _album.SongsId
                    .ToList();

                foreach (var id in songs)
                {
                    var song = _context.Songs
                        .Find(id);

                    if (song != null)
                    {
                        var card = UIElementsFactory
                            .CreateSongCardArtist(this, song);
                        SongsList.Children.Add(card);
                    }
                }
            }
        }
    }
}
