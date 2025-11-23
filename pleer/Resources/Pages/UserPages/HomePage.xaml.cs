using pleer.Models.DatabaseContext;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;

namespace pleer.Resources.Pages.UserPages
{
    /// <summary>
    /// Логика взаимодействия для HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;

        Listener _listener;

        Border _selectedCard;

        public HomePage(ListenerMainWindow listenerMain, Listener listener)
        {
            InitializeComponent();

            _listenerMain = listenerMain;
            _listener = listener;

            FillingPopularSongsControl();
            FillingPopularAlbumsControl();
        }
        
        void FillingPopularSongsControl()
        {
            PopularSongsControl.Children.Clear();

            var songs = _context.Songs
                .OrderByDescending(s => s.TotalPlays)
                .Take(5)
                .ToList();

            foreach (var song in songs)
            {
                var card = _listener != null
                    ? UIElementsFactory.CreateSongCard(song, _listener, _listenerMain.SongCard_Click)
                    : UIElementsFactory.CreateSongCard(song, _listenerMain.SongCard_Click);

                PopularSongsControl.Children.Add(card);
            }
        }

        public void CardPlaying(Border card)
        {
            if (_selectedCard != null)
                SetCardTitleColor(_selectedCard, "#eeeeee");

            SetCardTitleColor(card, "#90ee90");

            _selectedCard = card;
        }

        private void SetCardTitleColor(Border card, string hexColor)
        {
            if (card?.Child is not Grid grid) return;

            var infoPanel = grid.Children.OfType<StackPanel>().FirstOrDefault();

            if (infoPanel?.Children.Count > 0 && infoPanel.Children[0] is TextBlock titleTextBlock)
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);
                titleTextBlock.Foreground = new SolidColorBrush(color);
            }
        }

        void FillingPopularAlbumsControl()
        {
            PopularAlbumsControl.Items.Clear();

            var albums = _context.Albums
                .OrderByDescending(a => a.TotalPlays)
                .Take(6)
                .ToList();


            foreach (var album in albums)
            {
                var card = UIElementsFactory.CreateCollectionCard(album, _listenerMain.AlbumCard_Click);
                PopularAlbumsControl.Items.Add(card);
            }
        }
    }
}
