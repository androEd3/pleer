using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using pleer.Models.Service;

namespace pleer.Resources.Pages.Collections
{
    public partial class CollectionsList : Page
    {
        DBContext _context = new();

        ArtistMainWindow _artistMain;
        ListenerMainWindow _listenerMain;

        Artist _artist = new(); 
        Listener _listener = new();


        public CollectionsList(ArtistMainWindow main, Artist artist)
        {
            InitializeComponent();

            _artistMain = main;
            _artist = artist;

            LoadReleases();
        }

        public CollectionsList(ListenerMainWindow main, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;

            LoadMediaLibrary();
        }

        void LoadReleases()
        {
            ListenerCollection.Visibility = Visibility.Collapsed;

            AlbumContent.Navigate(new OpenCollection());

            ReleasesList.Children.Clear();

            if (_artist == null)
                return;

            var albums = _context.Albums
                .Where(u => u.CreatorId == _artist.Id)
                .ToArray();

            if (albums.Any())
            {
                NoAnyAlbumsPanel.Visibility = Visibility.Collapsed;

                foreach (var album in albums)
                {
                    var card = new UIElement();

                    if (_artistMain != null)
                        card = UIElementsFactory
                            .CreateCollectionCard(album, AlbumCard_Click);
                    if (_listenerMain != null)
                        card = UIElementsFactory
                            .CreateCollectionCard(album, _listenerMain.AlbumCard_Click);

                    ReleasesList.Children.Add(card);
                }

                NoAnyAlbumsPanel.Children.Remove(CreateNewAlbum);
                ReleasesList.Children.Add(CreateNewAlbum);
            }
        }

        void LoadMediaLibrary()
        {
            ArtistCollection.Visibility = Visibility.Collapsed;

            MediaLibraryList.Children.Clear();

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
                    var card = UIElementsFactory.CreateCollectionCard(playlist, _listenerMain.PlaylistCard_Click);
                    MediaLibraryList.Children.Add(card);
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

            LoadMediaLibrary();
        }

        public void AlbumCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Album album)
            {
                AlbumContent.Navigate(new OpenCollection(_artistMain, album));
            }
        }

        private void CreateNewAlbum_Click(object sender, MouseButtonEventArgs e)
        {
            _artistMain.OperationField.Navigate(new CreateAlbum(_artistMain, _artist));
        }
    }
}
