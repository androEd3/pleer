using pleer.Models.Users;
using pleer.Resources.Pages.AlbumsAndPlaylists;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;

namespace pleer.Resources.Pages.UserPages
{
    public partial class MediaLibrary : Page
    {
        AlbumsList _albumsList;

        public MediaLibrary(ListenerMainWindow main, Listener listener)
        {
            InitializeComponent();

            _albumsList = new AlbumsList(main, listener);

            ShowAlbumsList();
        }

        void ShowAlbumsList()
        {
            MediaLibraryAlbumsList.Navigate(_albumsList);
        }

        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            _albumsList.CreatePlaylist();
        }
    }
}
