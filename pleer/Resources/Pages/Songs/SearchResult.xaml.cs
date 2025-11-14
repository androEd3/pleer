using pleer.Models.DatabaseContext;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;

namespace pleer.Resources.Pages.Songs
{
    public partial class SearchResult : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;
        Listener _listener;

        string _searchBarContent;

        public SearchResult(ListenerMainWindow main, Listener listener, string searchBarContent)
        {
            InitializeComponent();

            _listenerMain = main;
            _listener = listener;
            _searchBarContent = searchBarContent;

            LoadSongsList();
        }

        public void LoadSongsList()
        {
            SongsList.Children.Clear();

            var songs = _context.Songs
                .Where(s => s.Title.Contains(_searchBarContent))
                .ToArray();

            if (songs.Length == 0)
            {
                NoContent(); return;
            }

            foreach (var song in songs)
            {
                var card = UIElementsFactory.CreateSongCard(song, _listener, _listenerMain.SongCard_Click);
                SongsList.Children.Add(card);
            }
        }

        void NoContent()
        {
            var infoPanel = new TextBlock()
            {
                Text = "По запросу ничего не найдено",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 15, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = TryFindResource("SmallInfoPanel") as Style,
            };
            SongsList.Children.Add(infoPanel);
        }
    }
}
