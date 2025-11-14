using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Pages.Collections;
using pleer.Resources.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace pleer.Resources.Pages.UserPages
{
    /// <summary>
    /// Логика взаимодействия для HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;

        Listener _listener;

        public HomePage(ListenerMainWindow listenerMain, Listener listener)
        {
            InitializeComponent();

            _listenerMain = listenerMain;
            _listener = listener;

            FillingPopularSongsControl();
            FillingPopularAlbumsControl();
        }

        void FillingPopularSongsControl()
        {
            PopularSongsControl.Children.Clear();

            var songs = _context.Songs
                .ToList();

            foreach (var song in songs)
            {
                var card = UIElementsFactory.CreateSongCard(song, _listenerMain.SongCard_Click);
                PopularSongsControl.Children.Add(card);
            }
        }

        void FillingPopularAlbumsControl()
        {
            PopularAlbumsControl.Items.Clear();

            var albums = _context.Albums
                .ToList();

            foreach (var album in albums)
            {
                var card = UIElementsFactory.CreateCollectionCard(album, _listenerMain.AlbumCard_Click);
                PopularAlbumsControl.Items.Add(card);
            }
        }
    }
}
