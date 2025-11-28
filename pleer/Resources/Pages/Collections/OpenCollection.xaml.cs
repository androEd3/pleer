using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Pages.UserPages;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public OpenCollection(CollectionsList collectionMain, ListenerMainWindow main, Album album, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _collectionMain = collectionMain;
            _listener = listener;
            _album = album;

            LoadAlbumMetadata();

            Unloaded += OpenCollectionPage_Unloaded;

            PlaylistFunctionalPanel.Visibility = Visibility.Collapsed;
        }


        public OpenCollection(ListenerMainWindow main, Album album, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;
            _album = album;

            LoadAlbumMetadata();

            Unloaded += OpenCollectionPage_Unloaded;

            PlaylistFunctionalPanel.Visibility = Visibility.Collapsed;
        }
        
        public OpenCollection(CollectionsList collectionMain, ArtistMainWindow artistMain, Album album)
        {
            InitializeComponent();

            _artistMain = artistMain;
            _collectionMain = collectionMain;
            _album = album;

            LoadAlbumMetadata();
        }

        private void OpenCollectionPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _listenerMain._currentAlbum = null;
        }

        void LoadPlaylistData()
        {
            if (_playlist == null)
                return;

            var refreashedPlaylist = _context.Playlists
                .FirstOrDefault(p => p.Id == _playlist.Id);

            // metadata
            LoadPlaylistMetadata(refreashedPlaylist);

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
            CreatonDate.Text = refreashedPlaylist.CreatedAt.ToString("d MMM yyyy");

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

            if (_artistMain != null)
                AlbumContent.Margin = new Thickness(0, 0, 0, 90);

            var refreashedAlbum = _context.Albums
                .FirstOrDefault(p => p.Id == _album.Id);

            if (refreashedAlbum == null)
                return;

            LoadAlbumMetadata(refreashedAlbum);

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

            //Songs list
            LoadAlbumSongs();
        }

        void LoadPlaylistMetadata(Playlist playlist)
        {
            if (playlist == null)
                return;

            AlbumName.Text = playlist.Title ?? "Неизвестно";
            ArtistName.Text = _context.Listeners.Find(playlist.CreatorId).Name ?? "Неизвестен";

            CollectionType.Text = "Плейлист";

            if (!string.IsNullOrEmpty(playlist.Description))
            {
                DescriptionText.Text = "Описание:\n" + playlist.Description;
                DescriptionText.Visibility = Visibility.Visible;
            }

            var cover = _context.PlaylistCovers
               .Find(playlist.CoverId);
            LoadCover(cover.FilePath);
        }

        void LoadAlbumMetadata(Album album)
        {
            //title & subtitle & type
            if (album == null)
                return;

            var artist = _context.Artists.Find(album.CreatorId);

            ArtistName.Tag = artist;

            AlbumName.Text = album.Title ?? "Неизвестно";
            ArtistName.Text = artist.Name ?? "Неизвестен";

            CollectionType.Text = "Альбом";

            var cover = _context.AlbumCovers
               .Find(album.CoverId);
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

                int songIndex = 1;
                foreach (var songId in songs)
                {
                    var song = _context.Songs.Find(songId);

                    if (song != null)
                    {
                        var card = _listener != null
                            ? UIElementsFactory.CreateSongCard(song, songIndex, _listener, _listenerMain.SongCard_Click)
                            : UIElementsFactory.CreateSongCard(song, songIndex, _listenerMain.SongCard_Click);

                        songIndex++;
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
                        int songIndex = songs.IndexOf(song.Id);

                        Border card;

                        if (_artistMain != null)
                            card = UIElementsFactory.CreateSongCard(song, songIndex, _artistMain.SongCard_Click);
                        else if (_listener != null)
                            card = UIElementsFactory.CreateSongCard(song, songIndex, _listener, _listenerMain.SongCard_Click);
                        else
                            card = UIElementsFactory.CreateSongCard(song, songIndex, _listenerMain.SongCard_Click);

                        if (card != null)
                            SongsList.Children.Add(card);
                    }
                }
            }
        }

        private void EditPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (_playlist != null)
                EditPlaylist();

            else if (_album != null)
                EditAlbum();
        }

        void EditAlbum()
        {
            var album = _context.Albums.Find(_album.Id);

            new PlaylistEditWindow(album).ShowDialog();
            _collectionMain.LoadReleases();
        }

        void EditPlaylist()
        {
            var playlist = _context.Playlists.Find(_playlist.Id);

            new PlaylistEditWindow(playlist).ShowDialog();
            _collectionMain.LoadMediaLibrary();
        }

        private void DeletePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                         $"Вы уверены, что хотите удалить {(_album != null ? _album.Title : _playlist.Title)}?",
                         "Подтверждение удаления",
                         MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (_playlist != null)
                    DeletePlaylist();

                else if (_album != null)
                    DeleteAlbum();
            }
        }

        void DeleteAlbum()
        {
            var album = _context.Albums.Find(_album.Id);

            _context.Albums.Remove(album);
            _context.SaveChanges();

            _collectionMain.LoadReleases();
        }

        void DeletePlaylist()
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

        private void ArtistName_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is Artist artist)
            {
                if (_listenerMain != null)
                {
                    if (_listener != null)
                        _listenerMain.CenterField.Navigate(new ArtistProfilePage(_listenerMain, artist, _listener));
                    else
                        _listenerMain.CenterField.Navigate(new ArtistProfilePage(_listenerMain, artist));
                }
            }
        }
    }
}
