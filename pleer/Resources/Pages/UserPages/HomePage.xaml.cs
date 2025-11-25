using Microsoft.EntityFrameworkCore;
using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Pages.Collections;
using pleer.Resources.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace pleer.Resources.Pages.UserPages
{
    /// <summary>
    /// Логика взаимодействия для HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;
        ArtistMainWindow _artistMain;

        Listener _listener;
        Artist _artist;

        public HomePage(ListenerMainWindow listenerMain, Listener listener)
        {
            InitializeComponent();

            _listenerMain = listenerMain;
            _listener = listener;

            FillingPopularSongsControl();
            FillingPopularAlbumsControl();
        }

        public HomePage(ListenerMainWindow listenerMain, Artist artist)
        {
            InitializeComponent();

            _listenerMain = listenerMain;
            _artist = artist;

            FillingArtistPopularSongsControl();
            FillingArtistPopularAlbumsControl();
        }

        public HomePage(ListenerMainWindow listenerMain, Artist artist, Listener listener)
        {
            InitializeComponent();

            _listenerMain = listenerMain;
            _artist = artist;
            _listener = listener;

            FillingArtistPopularSongsControl();
            FillingArtistPopularAlbumsControl();
        }

        public HomePage(ArtistMainWindow artistMain, Artist artist)
        {
            InitializeComponent();

            _artistMain = artistMain;
            _artist = artist;

            FillingArtistPopularSongsControl();
            FillingArtistPopularAlbumsControl();
        }

        void FillingPopularSongsControl()
        {
            PopularSongsControl.Children.Clear();

            var songs = _context.Songs
                    .OrderByDescending(s => s.TotalPlays)
                    .Take(5)
                    .ToList();

            if (!songs.Any())
            {
                string message = "Пока что нет песен";
                UIElementsFactory.NoContent(message, PopularSongsControl);
                return;
            }

            foreach (var song in songs)
            {
                int songIndex = songs.IndexOf(song);

                var card = _listener != null
                    ? UIElementsFactory.CreateSongCard(song, songIndex, _listener, _listenerMain.SongCard_Click)
                    : UIElementsFactory.CreateSongCard(song, songIndex, _listenerMain.SongCard_Click);

                PopularSongsControl.Children.Add(card);
            }
        }
        
        void FillingPopularAlbumsControl()
        {
            PopularAlbumsControl.Items.Clear();

            var albums = _context.Albums
                .OrderByDescending(a => a.TotalPlays)
                .Take(6)
                .ToList();

            if (!albums.Any())
            {
                string message = "Пока что нет выпусков";
                UIElementsFactory.NoContent(message, NotFoundAlbumsPanel);
                return;
            }

            foreach (var album in albums)
            {
                var card = UIElementsFactory.CreateCollectionCard(album, _listenerMain.AlbumCard_Click);
                PopularAlbumsControl.Items.Add(card);
            }
        }

        void FillingArtistPopularSongsControl()
        {
            var artistData = _context.Artists
                    .Where(a => a.Id == _artist.Id)
                    .Select(a => new
                    {
                        Artist = a,
                        a.ArtistsAlbums,
                    })
                    .FirstOrDefault();

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
            var songs = artistsSong
                .OrderByDescending(s => s.TotalPlays)
                .Take(5)
                .ToList();

            if (!songs.Any())
            {
                string message = "Пока что нет песен";
                UIElementsFactory.NoContent(message, PopularSongsControl);
                return;
            }

            foreach (var song in songs)
            {
                int songIndex = songs.IndexOf(song);

                Border card = new();

                if (_listenerMain != null)
                    card = _listener != null
                        ? UIElementsFactory.CreateSongCard(song, songIndex, _listener, _listenerMain.SongCard_Click)
                        : UIElementsFactory.CreateSongCard(song, songIndex, _listenerMain.SongCard_Click);
                else if (_artistMain != null)
                {
                    card = UIElementsFactory.CreateSongCard(song, songIndex, _artistMain.SongCard_Click);
                    card.Tag = null;
                }

                PopularSongsControl.Children.Add(card);
            }
        }

        void FillingArtistPopularAlbumsControl()
        {
            var artistData = _context.Artists
                .Where(a => a.Id == _artist.Id)
                .Select(a => new
                {
                    Artist = a,
                    a.ArtistsAlbums,
                })
                .FirstOrDefault();

            var albums = artistData.ArtistsAlbums
                .OrderByDescending(s => s.TotalPlays)
                .ToList();

            if (!albums.Any())
            {
                string message = "Пока что нет выпусков";
                UIElementsFactory.NoContent(message, NotFoundAlbumsPanel);
                return;
            }

            foreach (var album in albums)
            {
                Border card = new();

                if (_artistMain != null)
                    card = UIElementsFactory.CreateCollectionCard(album, AlbumCard_Click);

                else if (_listenerMain != null)
                    card = UIElementsFactory.CreateCollectionCard(album, _listenerMain.AlbumCard_Click);

                PopularAlbumsControl.Items.Add(card);
            }
        }

        public void AlbumCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Album album)
            {
                var collectionList = new CollectionsList(_artistMain, _artist);
                collectionList._album = album;
                _artistMain.OperationField.Navigate(collectionList);
            }
        }
    }
}
