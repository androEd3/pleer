using pleer.Models.Media;
using pleer.Models.Users;
using pleer.Resources.Pages;
using pleer.Resources.Pages.ArtistPages;
using pleer.Resources.Pages.UserPages.FullWindow;
using pleer.Resources.Windows;

namespace pleer.Models.ModelsUI
{
    public class NavigateMethods
    {
        //User window
        public static void OpenPlaylist(ListenerMainWindow main, Playlist playlist, Listener listener)
        {
            main.CenterField.Navigate(new OpenAlbum(main, playlist, listener));
        }

        public static void OpenSongsSimpleList(ListenerMainWindow main, Listener listener)
        {
            main.CenterField.Navigate(new SimpleSongList(main, listener));
        }

        public static void OpenListenerProfile(ListenerMainWindow main, Listener listener)
        {
            main.FullWindow.Navigate(new ProfilePage(main, listener));
        }

        public static void OpenListenerChangePasswordPage(ListenerMainWindow main, Listener listener)
        {
            main.FullWindow.Navigate(new ChangePasswordPage(main, listener));
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

        public static void OpenArtistProfile(ArtistMainWindow main, Artist artist)
        {
            main.FullWindow.Navigate(new ProfilePage(main, artist));
        }

        public static void OpenArtistChangePasswordPage(ArtistMainWindow main, Artist artist)
        {
            main.FullWindow.Navigate(new ChangePasswordPage(main, artist));
        }

        //Login window
        public static void OpenListenerLoginPage(ListenerMainWindow main)
        {
            main.FullWindow.Navigate(new LoginPage(main));
        }
        public static void OpenArtistLoginPage(ArtistMainWindow main)
        {
            main.FullWindow.Navigate(new LoginPage(main));
        }

        public static void OpenListenerRegistrationPage(ListenerMainWindow main)
        {
            main.FullWindow.Navigate(new RegistrationPage(main));
        }
        public static void OpenArtistRegistrationPage(ArtistMainWindow main)
        {
            main.FullWindow.Navigate(new RegistrationPage(main));
        }
    }
}
