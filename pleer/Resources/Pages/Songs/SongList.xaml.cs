using pleer.Models.CONTEXT;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows.Controls;

namespace pleer.Resources.Pages.Songs
{
    public partial class SongList : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;

        Listener _listener;

        public SongList(ListenerMainWindow main, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;

            LoadSongsList();
        }

        public void LoadSongsList()
        {
            SongsList.Children.Clear();

            var songs = _context.Songs
                .ToArray();

            foreach (var song in songs)
            {
                var card = UIElementsFactory
                    .CreateSongCardListener(_listenerMain, _listener, song);
                SongsList.Children.Add(card);
            }
        }
    }
}
