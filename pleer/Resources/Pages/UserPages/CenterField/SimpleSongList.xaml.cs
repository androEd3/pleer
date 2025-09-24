using Microsoft.EntityFrameworkCore;
using pleer.Models.CONTEXT;
using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Логика взаимодействия для SimpleSongList.xaml
    /// </summary>
    public partial class SimpleSongList : Page
    {
        dbContext _context = new dbContext();

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
