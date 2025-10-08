using pleer.Models.CONTEXT;
using pleer.Models.DB_Models;
using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace pleer.Resources.Pages
{
    /// <summary>
    /// Логика взаимодействия для SimplePlaylistsList.xaml
    /// </summary>
    public partial class SimplePlaylistsList : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;

        Listener _listener;

        public SimplePlaylistsList(ListenerMainWindow main, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;

            LoadPlaylistsList();
        }

        void LoadPlaylistsList()
        {
            PlaylistsList.Children.Clear();

            if (_listener == null)
                return;

            var links = _context.ListenerPlaylistsLinks
                .Where(u => u.ListenerId == _listener.Id)
                .Select(u => u.PlaylistId)
                .ToArray();

            foreach (var id in links)
            {
                var playlist = _context.Playlists.Find(id);

                var card = UIServiceMethods.CreatePlaylistCard(this, playlist);
                PlaylistsList.Children.Add(card);
            }
        }

        public void PlaylistCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Playlist playlist)
            {
                NavigateMethods.OpenPlaylist(_listenerMain, playlist, _listener);
            }
        }

        private void CreatePlaylictButton_Click(object sender, RoutedEventArgs e)
        {
            if (_listener == null)
            {
                MessageBox.Show("Зайдите в аккаунт чтобы воспользоваться данной функцией");
                return;
            }

            ServiceMethods.AddPlaylistWithLink(_listener);

            LoadPlaylistsList();
        }
    }
}
