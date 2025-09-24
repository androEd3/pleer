using pleer.Models.CONTEXT;
using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
    /// Логика взаимодействия для OpenAlbum.xaml
    /// </summary>
    public partial class OpenAlbum : Page
    {
        dbContext _context = new dbContext();

        UserMainWindow _userMainWindow;

        User _user;

        public OpenAlbum(UserMainWindow main, Playlist playlist, User user)
        {
            InitializeComponent();

            _userMainWindow = main;

            _user = user;

            LoadSongsList(playlist);
        }

        void LoadPlaylistMetadata(Playlist playlist)
        {
            var user = _context.Users.Find(playlist.CreatorId);

            AlbumName.Text = playlist.Title;
            ArtistName.Text = user.Name;

            var cover = _context.AlbumCovers.Find(playlist.AlbumCoverId);

            if (string.IsNullOrEmpty(cover.FilePath))
            {
                AlbumCoverCenterField.Source = new BitmapImage(new Uri("..\\Resources\\ServiceImages\\NoMediaImage.png"));
            }
            else
                AlbumCoverCenterField.Source = UIServiceMethods.DecodePhoto(cover.FilePath, 90);
        }

        //Create lists
        public void LoadSongsList(Playlist playlist)
        {
            SongsList.Children.Clear();

            LoadPlaylistMetadata(playlist);

            var songs = playlist.SongsId
                .ToArray();

            foreach (var id in songs)
            {
                var song = _context.Songs.Find(id);

                var card = UIServiceMethods.CreateSongCard(_userMainWindow, _user, song);
                SongsList.Children.Add(card);
            }
        }
    }
}
