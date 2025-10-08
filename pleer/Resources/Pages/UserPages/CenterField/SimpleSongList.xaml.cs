using Microsoft.EntityFrameworkCore;
using pleer.Models.CONTEXT;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows.Controls;

namespace pleer.Resources.Pages
{
    /// <summary>
    /// Логика взаимодействия для SimpleSongList.xaml
    /// </summary>
    public partial class SimpleSongList : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;

        Listener _listener;

        public SimpleSongList(ListenerMainWindow main, Listener listener)
        {
            InitializeComponent();

            _listenerMain = main;

            _listener = listener;

            LoadSongsList();
        }

        public async void LoadSongsList()
        {
            SongsList.Children.Clear();

            var songs = await _context.Songs
                .ToArrayAsync();

            foreach (var song in songs)
            {
                var card = UIServiceMethods.CreateSongCard(_listenerMain, _listener, song);
                SongsList.Children.Add(card);
            }
        }
    }
}
