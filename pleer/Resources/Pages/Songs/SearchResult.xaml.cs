using pleer.Models.DatabaseContext;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace pleer.Resources.Pages.Songs
{
    public partial class SearchResult : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;
        Listener _listener;

        string _searchBarContent;

        public SearchResult(ListenerMainWindow main, Listener listener, string searchBarContent)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;
            _searchBarContent = searchBarContent;

            LoadSongsList();
        }

        public void LoadSongsList()
        {
            SongsList.Children.Clear();

            SearchSongsButton.Background = ColorConvert("#eee");
            SearchSongsButton.Foreground = ColorConvert("#333");

            SearchPlaylistsButton.Background = ColorConvert("#333");
            SearchPlaylistsButton.Foreground = ColorConvert("#eee");

            SearchArtistsButton.Background = ColorConvert("#333");
            SearchArtistsButton.Foreground = ColorConvert("#eee");

            var songs = _context.Songs
                .Where(s => s.Title.Contains(_searchBarContent))
                .ToList();

            if (!songs.Any())
            {
                string message = "По запросу ничего не найдено";
                UIElementsFactory.NoContent(message, SongsList);
                return;
            }

            foreach (var song in songs)
            {
                int songId = songs.IndexOf(song);

                var card = UIElementsFactory.CreateSongCard(song, songId, _listener, _listenerMain.SongCard_Click);
                SongsList.Children.Add(card);
            }
        }
        
        public void LoadPlaylistsList()
        {
            SongsList.Children.Clear();

            SearchSongsButton.Background = ColorConvert("#333");
            SearchSongsButton.Foreground = ColorConvert("#eee");

            SearchPlaylistsButton.Background = ColorConvert("#eee");
            SearchPlaylistsButton.Foreground = ColorConvert("#333");

            SearchArtistsButton.Background = ColorConvert("#333");
            SearchArtistsButton.Foreground = ColorConvert("#eee");

            var albums = _context.Albums
                .Where(s => s.Title.Contains(_searchBarContent))
                .ToArray();

            if (albums.Length == 0)
            {
                string message = "По запросу ничего не найдено";
                UIElementsFactory.NoContent(message, SongsList);
                return;
            }

            foreach (var album in albums)
            {
                var card = UIElementsFactory.CreateCollectionCard(album, _listenerMain.AlbumCard_Click);
                SongsList.Children.Add(card);
            }
        }

        public void LoadArtistsList()
        {
            SongsList.Children.Clear();

            SearchSongsButton.Background = ColorConvert("#333");
            SearchSongsButton.Foreground = ColorConvert("#eee");

            SearchPlaylistsButton.Background = ColorConvert("#333");
            SearchPlaylistsButton.Foreground = ColorConvert("#eee");

            SearchArtistsButton.Background = ColorConvert("#eee");
            SearchArtistsButton.Foreground = ColorConvert("#333");

            var artists = _context.Artists
                .Where(a => a.Name.Contains(_searchBarContent))
                .ToList();

            if (!artists.Any())
            {
                string message = "По запросу ничего не найдено";
                UIElementsFactory.NoContent(message, SongsList);
                return;
            }

            foreach (var artist in artists)
            {
                int artistId = artists.IndexOf(artist);

                var card = UIElementsFactory.CreateArtistCard(artist, _listenerMain.ArtistCard_Click);
                SongsList.Children.Add(card);
            }
        }


        SolidColorBrush ColorConvert(string hex)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }

        private void SearchSongsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSongsList();
        }

        private void SearchPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPlaylistsList();
        }

        private void SearchArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadArtistsList();
        }
    }
}
