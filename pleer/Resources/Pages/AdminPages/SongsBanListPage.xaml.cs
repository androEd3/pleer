using Microsoft.EntityFrameworkCore;
using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Service;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace pleer.Resources.Pages.AdminPages
{
    /// <summary>
    /// Логика взаимодействия для SongsBanListPage.xaml
    /// </summary>
    public partial class SongsBanListPage : Page
    {
        DBContext _context = new();

        string _searchParameter = string.Empty;

        public SongsBanListPage()
        {
            InitializeComponent();

            LoadSongsList();
        }

        public void LoadSongsList()
        {
            SongsPanel.Children.Clear();

            List<Song> songs;

            if (string.IsNullOrEmpty(_searchParameter))
            {
                songs = _context.Songs
                    .ToList();
            }
            else
            {
                songs = _context.Songs
                    .Include(s => s.Album)
                        .ThenInclude(a => a.Creator)
                    .Include(s => s.Album.Cover)
                    .Where(s => s.Title.Contains(_searchParameter) ||
                                s.Album.Creator.Name.Contains(_searchParameter) ||
                                s.Album.Title.Contains(_searchParameter))
                    .ToList();
            }

            if (!songs.Any())
            {
                string message = "Песен не найдено";
                UIElementsFactory.NoContent(message, SongsPanel);
                return;
            }

            foreach (var song in songs)
            {
                int songId = songs.IndexOf(song);

                var card = UIElementsFactory.CreateSongCard(songId, song, BlockSong);
                SongsPanel.Children.Add(card);
            }

            TotlaSongs.Text = $"Найдено песен: {songs.Count}";
        }

        public void BlockSong(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Song song)
            {
                try
                {
                    var refreshedSong = _context.Songs.Find(song.Id);

                    refreshedSong.Status = !refreshedSong.Status;
                    _context.SaveChanges();

                    LoadSongsList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось заблокировать/разблокировать песню {song}. Ошибка: {ex.Message}");
                }
            }
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

            LoadSongsList();
        }
    }
}
