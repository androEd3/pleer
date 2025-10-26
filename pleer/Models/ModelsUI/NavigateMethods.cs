using pleer.Models.Users;
using pleer.Resources.Pages.GeneralPages;
using pleer.Resources.Windows;

namespace pleer.Models.ModelsUI
{
    public class NavigateMethods
    {
        //User window
        public static void OpenListenerProfile(ListenerMainWindow main, Listener listener)
        {
            main.FullWindow.Navigate(new ProfilePage(main, listener));
        }

        //Artist window
        public static void OpenArtistProfile(ArtistMainWindow main, Artist artist)
        {
            main.FullWindow.Navigate(new ProfilePage(main, artist));
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
    }
}
