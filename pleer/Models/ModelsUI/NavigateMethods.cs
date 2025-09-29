using pleer.Models.CONTEXT;
using pleer.Models.Media;
using pleer.Models.Users;
using pleer.Resources.Pages;
using pleer.Resources.Pages.ArtistPages;
using pleer.Resources.Pages.UserPages.FullWindow;
using pleer.Resources.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pleer.Models.ModelsUI
{
    public class NavigateMethods
    {
        //User window
        public static void OpenPlaylist(UserMainWindow main, Playlist playlist, User user)
        {
            main.CenterField.Navigate(new OpenAlbum(main, playlist, user));
        }

        public static void OpenSongsSimpleList(UserMainWindow main, User user)
        {
            main.CenterField.Navigate(new SimpleSongList(main, user));
        }

        //Artist window
        public static void CreateAlbum(ArtistMainWindow main, Artist artist)
        {
            main.OperationField.Navigate(new CreateAlbum(main, artist));
        }

        public static void AddSongsToAlbum(ArtistMainWindow main, Album album, AlbumCover cover)
        {
            main.OperationField.Navigate(new AddSongsToAlbum(main, album, cover));
        }

        //Login window
        public static void OpenUserLoginPage(UserMainWindow main)
        {
            main.FullWindow.Navigate(new LoginPage(main));
        }
        public static void OpenArtistLoginPage(ArtistMainWindow main)
        {
            main.FullWindow.Navigate(new LoginPage(main));
        }

        public static void OpenUserRegistrationPage(UserMainWindow main)
        {
            main.FullWindow.Navigate(new RegistrationPage(main));
        }
        public static void OpenArtistRegistrationPage(ArtistMainWindow main)
        {
            main.FullWindow.Navigate(new RegistrationPage(main));
        }
    }
}
