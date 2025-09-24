using pleer.Models.CONTEXT;
using pleer.Models.DB_Models;
using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pleer.Resources.Pages
{
    /// <summary>
    /// Логика взаимодействия для SimplePlaylistsList.xaml
    /// </summary>
    public partial class SimplePlaylistsList : Page
    {
        dbContext _context = new dbContext();

        UserMainWindow _mainWindow;

        User _user;

        public SimplePlaylistsList(UserMainWindow main, User user)
        {
            InitializeComponent();

            _user = user;
            _mainWindow = main;

            LoadPlaylistsList();
        }

        void LoadPlaylistsList()
        {
            PlaylistsList.Children.Clear();

            if (_user == null)
                return;

            var playlists = _context.UserPlaylistsLinks
                .Where(u => u.UserId == _user.Id)
                .Select(u => u.Playlist)
                .ToArray();

            foreach (var playlist in playlists)
            {
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

            DBServiceMethods.AddPlaylistWithLink(_user);

            LoadPlaylistsList();
        }
    }
}
