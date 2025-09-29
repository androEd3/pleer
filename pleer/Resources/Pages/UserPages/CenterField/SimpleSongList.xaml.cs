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

        UserMainWindow _userMainWindow;

        User _user;

        public SimpleSongList(UserMainWindow main, User user)
        {
            InitializeComponent();

            _userMainWindow = main;

            _user = user;

            LoadSongsList();
        }

        public async void LoadSongsList()
        {
            SongsList.Children.Clear();

            var songs = await _context.Songs
                .ToArrayAsync();

            foreach (var song in songs)
            {
                var card = UIServiceMethods.CreateSongCard(_userMainWindow, _user, song);
                SongsList.Children.Add(card);
            }
        }
    }
}
