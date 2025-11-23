using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Pages.UserPages;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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
        CollectionsList _collectionMain;

        Album _album;
        Playlist _playlist;

        Listener _listener;

        public OpenCollection(CollectionsList collectionMain, ListenerMainWindow listenerMain, Playlist playlist, Listener listener)
        {
            InitializeComponent();

            _listenerMain = listenerMain;
            _collectionMain = collectionMain;
            _listener = listener;
            _playlist = playlist;

            LoadPlaylistData();
        }

        public OpenCollection(ListenerMainWindow main, Album album, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;
            _album = album;

            LoadAlbumMetadata();

            PlaylistFunctionalPanel.Visibility = Visibility.Collapsed;
        }

        public OpenCollection(CollectionsList collectionMain, ListenerMainWindow main, Album album, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _collectionMain = collectionMain;
            _listener = listener;
            _album = album;

            LoadAlbumMetadata();

            PlaylistFunctionalPanel.Visibility = Visibility.Collapsed;
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

            PlaylistFunctionalPanel.Visibility = Visibility.Collapsed;
        }

        void LoadPlaylistData()
        {
            var refreashedPlaylist = _context.Playlists
                .FirstOrDefault(p => p.Id == _playlist.Id);

            var listener = _context.Listeners
                .FirstOrDefault(l => l.Id == _playlist.CreatorId);

            //Tracks count
            TracksCount.Text = $"Треков: {refreashedPlaylist.SongsId.Count}";

            //totaliti time
            var songs = new List<Song>();
            foreach (var songId in refreashedPlaylist.SongsId)
            {
                songs.Add(_context.Songs.Find(songId));
            }
            SummaryDuration.Text = " | Длительность: " + UIElementsFactory.FormattingTotalTime(songs);

            //creation date
            CreatonDate.Text = refreashedPlaylist.CreationDate.ToString("d MMM yyyy");

            // metadata
            LoadPlaylistMetadata(refreashedPlaylist, listener);

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
            var songs = new List<Song>();
            foreach (var songId in refreashedAlbum.SongsId)
            {
                songs.Add(_context.Songs.Find(songId));
            }
            SummaryDuration.Text = " | Длительность: " + UIElementsFactory.FormattingTotalTime(songs);

            //creation date
            CreatonDate.Text = refreashedAlbum.ReleaseDate.ToString("d MMM yyyy");

            //cover
            var cover = _context.AlbumCovers
               .Find(refreashedAlbum.CoverId);
            LoadCover(cover.FilePath);

            //Songs list
            LoadAlbumSongs();
        }

        void LoadPlaylistMetadata(Playlist playlist, Listener listener)
        {
            AlbumName.Text = playlist.Title ?? "Неизвестно";
            ArtistName.Text = listener.Name ?? "Неизвестен";

            if (!string.IsNullOrEmpty(playlist.Description))
            {
                DescriptionText.Text = "Описание:\n" + playlist.Description;
                DescriptionText.Visibility = Visibility.Visible;
            }

            var cover = _context.PlaylistCovers
               .Find(playlist.CoverId);
            LoadCover(cover.FilePath);
        }

        void LoadCover(string filePath)
        {
            AlbumCoverCenterField.ImageSource = 
                UIElementsFactory
                    .DecodePhoto(filePath, (int)AlbumCoverCenterField.ImageSource.Width) ??
                UIElementsFactory
                    .DecodePhoto(InitializeData.GetDefaultCoverPath(), (int)AlbumCoverCenterField.ImageSource.Width);
        }

        //Create lists
        void LoadPlaylistSongs()
        {
            if (_playlist != null)
            {
                var songs = new List<int>();

                var refreshedPlaylist = _context.Playlists.Find(_playlist.Id);
                if (refreshedPlaylist != null)
                    songs = refreshedPlaylist.SongsId.ToList();

                foreach (var id in songs)
                {
                    var song = _context.Songs.Find(id);

                    if (song != null)
                    {
                        var card = _listener != null
                            ? UIElementsFactory.CreateSongCard(song, _listener, _listenerMain.SongCard_Click)
                            : UIElementsFactory.CreateSongCard(song, _listenerMain.SongCard_Click);

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
                        UIElement card;

                        if (_artistMain != null)
                            card = UIElementsFactory.CreateSongCard(song, _artistMain.SongCard_Click);
                        else if (_listener != null)
                            card = UIElementsFactory.CreateSongCard(song, _listener, _listenerMain.SongCard_Click);
                        else
                            card = UIElementsFactory.CreateSongCard(song, _listenerMain.SongCard_Click);

                        SongsList.Children.Add(card);
                    }
                }
            }
        }

        private void EditPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (_playlist != null)
            {
                var playlist = _context.Playlists.Find(_playlist.Id);

                new PlaylistEditWindow(playlist).ShowDialog();
                _collectionMain.LoadMediaLibrary();
                LoadPlaylistMetadata(playlist, _listener);
            }
            if (_album != null)
            {
                var album = _context.Albums.Find(_album.Id);

                new PlaylistEditWindow(album).ShowDialog();
                _collectionMain.LoadReleases();
                LoadAlbumMetadata();
            }
        }

        private void DeletePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                         "Вы уверены, что хотите удалить плейлист?",
                         "Подтверждение удаления",
                         MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (_playlist != null)
                {
                    var listener = _context.Listeners.Find(_listener.Id);
                    var playlist = _context.Playlists.Find(_playlist.Id);

                    var link = new ListenerPlaylistsLink()
                    {
                        ListenerId = listener.Id,
                        PlaylistId = playlist.Id
                    };
                    _context.ListenerPlaylistsLinks.Remove(link);

                    _context.Playlists.Remove(playlist);
                    _context.SaveChanges();

                    _collectionMain.LoadMediaLibrary();
                    _listenerMain.CenterField.Navigate(new HomePage(_listenerMain, listener));
                }

                if (_album != null)
                {
                    var album = _context.Albums.Find(_album.Id);

                    _context.Albums.Remove(album);
                    _context.SaveChanges();

                    _collectionMain.LoadReleases();
                }
            }
            else return;
        }
    }
}
