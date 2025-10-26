using pleer.Models.CONTEXT;
using pleer.Models.DB_Models;
using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Pages.GeneralPages;
using pleer.Resources.Pages.Songs;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace pleer.Resources.Pages.AlbumsAndPlaylists
{
    public partial class AlbumsList : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;
        ArtistMainWindow _artistMain;

        Listener _listener = new();
        Artist _artist = new();

        public  AlbumsList(ListenerMainWindow main, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;

            LoadListenerMediaLibrary();
        }

        public AlbumsList(ArtistMainWindow main, Artist artist)
        {
            InitializeComponent();

            _artistMain = main;
            _artist = artist;

            LoadArtistMediaLibrary();
        }

        void LoadListenerMediaLibrary()
        {
            NoAnyAlbumsPanel.Visibility = Visibility.Collapsed;

            MediaLibraryAlbumsList.Children.Clear();

            if (_listener == null)
                return;

            var links = _context.ListenerPlaylistsLinks
                .Where(u => u.ListenerId == _listener.Id)
                .Select(u => u.PlaylistId)
                .ToArray();

            foreach (var id in links)
            {
                var playlist = _context.Playlists.Find(id);

                if (playlist != null)
                {
                    var card = UIElementsFactory.CreatePlaylistCard(this, playlist);
                    MediaLibraryAlbumsList.Children.Add(card);
                }
            }
        }

        void LoadArtistMediaLibrary()
        {
            AlbumContent.Navigate(new OpenAlbum());

            MediaLibraryAlbumsList.Children.Clear();

            if (_artist == null)
                return;

            var albums = _context.Albums
                .Where(u => u.ArtistId == _artist.Id)
                .ToArray();

            if (albums.Any())
            {
                NoAnyAlbumsPanel.Visibility = Visibility.Collapsed;

                foreach (var album in albums)
                {
                    var card = UIElementsFactory.CreateAlbumCard(this, album);
                    MediaLibraryAlbumsList.Children.Add(card);
                }

                NoAnyAlbumsPanel.Children.Remove(CreateNewAlbum);
                MediaLibraryAlbumsList.Children.Add(CreateNewAlbum);
            }
        }

        public void PlaylistCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Playlist playlist)
            {
                _listenerMain.CenterField.Navigate(new OpenAlbum(_listenerMain, playlist, _listener));
            }
        }

        public void AlbumCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_artistMain != null)
            {
                if (sender is Border border && border.Tag is Album album)
                {
                    AlbumContent.Navigate(new OpenAlbum(album, _artist));
                }
            }

            if (_listenerMain != null)
            {
                if (sender is Border border && border.Tag is Album album)
                {
                    _listenerMain.CenterField.Navigate(new OpenAlbum(_listenerMain, album, _listener));
                }
            }
        }

        public void CreatePlaylist()
        {
            if (_listener == null)
            {
                MessageBox.Show("Зайдите в аккаунт чтобы воспользоваться данной функцией");
                return;
            }

            ServiceMethods.AddPlaylistWithLink(_listener);

            LoadListenerMediaLibrary();
        }

        private void CreateNewAlbum_Click(object sender, MouseButtonEventArgs e)
        {
            _artistMain.OperationField.Navigate(new CreateAlbum(_artistMain, _artist));
        }
    }
}
