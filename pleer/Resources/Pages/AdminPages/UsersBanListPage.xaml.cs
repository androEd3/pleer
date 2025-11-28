using Microsoft.EntityFrameworkCore;
using pleer.Models.DatabaseContext;
using pleer.Models.Service;
using pleer.Models.Users;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace pleer.Resources.Pages.AdminPages
{
    /// <summary>
    /// Логика взаимодействия для UsersBanListPage.xaml
    /// </summary>
    public partial class UsersBanListPage : Page
    {
        DBContext _context = new();

        bool _isListener;
        bool _isArtist;

        string _searchParameter = string.Empty;

        public UsersBanListPage()
        {
            InitializeComponent();

            LoadListenersList();
        }

        public void LoadListenersList()
        {
            UsersPanel.Children.Clear();

            _isArtist = false;
            _isListener = true;

            LoadListenersButton.Background = UIElementsFactory.ColorConvert("#eee");
            LoadListenersButton.Foreground = UIElementsFactory.ColorConvert("#333");

            LoadArtistsButton.Background = UIElementsFactory.ColorConvert("#333");
            LoadArtistsButton.Foreground = UIElementsFactory.ColorConvert("#eee");

            List<Listener> listeners;

            if (string.IsNullOrEmpty(_searchParameter))
            {
                listeners = _context.Listeners
                    .ToList();
            }
            else
            {
                listeners = _context.Listeners
                    .Where(s => s.Name.Contains(_searchParameter))
                    .ToList();
            }

            if (!listeners.Any())
            {
                string message = "Слушателей не найдено";
                UIElementsFactory.NoContent(message, UsersPanel);
                return;
            }

            foreach (var listener in listeners)
            {
                int listenerId = listeners.IndexOf(listener);

                var card = UIElementsFactory.CreateUserCard(listenerId, listener, BlockListener);
                UsersPanel.Children.Add(card);
            }

            TotlaUsers.Text = $"Найдено слушателей: {listeners.Count}";
        }

        public void LoadArtistsList()
        {
            UsersPanel.Children.Clear();

            _isArtist = true;
            _isListener = false;

            LoadListenersButton.Background = UIElementsFactory.ColorConvert("#333");
            LoadListenersButton.Foreground = UIElementsFactory.ColorConvert("#eee");

            LoadArtistsButton.Background = UIElementsFactory.ColorConvert("#eee");
            LoadArtistsButton.Foreground = UIElementsFactory.ColorConvert("#333");

            List<Artist> artists;

            if (string.IsNullOrEmpty(_searchParameter))
            {
                artists = _context.Artists
                    .ToList();
            }
            else
            {
                artists = _context.Artists
                    .Where(s => s.Name.Contains(_searchParameter))
                    .ToList();
            }

            if (!artists.Any())
            {
                string message = "Исполнителей не найдено";
                UIElementsFactory.NoContent(message, UsersPanel);
                return;
            }

            foreach (var artist in artists)
            {
                int artistId = artists.IndexOf(artist);

                var card = UIElementsFactory.CreateUserCard(artistId, artist, BlockArtist);
                UsersPanel.Children.Add(card);
            }

            TotlaUsers.Text = $"Найдено исполнителей: {artists.Count}";
        }

        public void BlockListener(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Listener listener)
            {
                try
                {
                    var refreshedListener = _context.Listeners.Find(listener.Id);

                    refreshedListener.Status = !refreshedListener.Status;
                    _context.SaveChanges();

                    LoadListenersList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось заблокировать/разблокировать слушателя {listener}. Ошибка: {ex.Message}");
                }
            }
        }

        public void BlockArtist(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Artist artist)
            {
                try
                {
                    var refreshedArtist = _context.Artists.Find(artist.Id);

                    refreshedArtist.Status = !refreshedArtist.Status;
                    _context.SaveChanges();

                    LoadArtistsList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось заблокировать/разблокировать исполнителя {artist}. Ошибка: {ex.Message}");
                }
            }
        }

        private void LoadListenersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadListenersList();
        }

        private void LoadArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadArtistsList();
        }

        // POISK
        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchContent();
        }

        private void Enter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchContent();
        }

        void SearchContent()
        {
            _searchParameter = SearchBar.Text;

            if (_isListener) LoadListenersList();
            else if (_isArtist) LoadArtistsList();
        }
    }
}
