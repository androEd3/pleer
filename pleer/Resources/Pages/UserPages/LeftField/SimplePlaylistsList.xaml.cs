using Microsoft.EntityFrameworkCore;
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

        UserMainWindow _mainWindow;

        User _user;

        public SimplePlaylistsList(UserMainWindow main, User user)
        {
            InitializeComponent();

            _mainWindow = main;
            _user = user;

            LoadPlaylistsList();
        }

        void LoadPlaylistsList()
        {
            PlaylistsList.Children.Clear();

            if (_user == null)
                return;

            var links = _context.UserPlaylistsLinks
                .Where(u => u.UserId == _user.Id)
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
                NavigateMethods.OpenPlaylist(_mainWindow, playlist, _user);
            }
        }

        private void CreatePlaylictButton_Click(object sender, RoutedEventArgs e)
        {
            if (_user == null)
            {
                MessageBox.Show("Зайдите в аккаунт чтобы воспользоваться данной функцией");
                return;
            }

            ServiceMethods.AddPlaylistWithLink(_user, _context, false);

            LoadPlaylistsList();
        }
    }
}
