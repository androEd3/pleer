using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Service;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace pleer.Resources.Pages.Songs
{
    /// <summary>
    /// Логика взаимодействия для ListenedHistoryPage.xaml
    /// </summary>
    public partial class ListenedHistoryPage : Page
    {
        DBContext _context = new();

        ListenerMainWindow _listenerMain;

        public ListenedHistoryPage(ListenerMainWindow listenerMain)
        {
            InitializeComponent();

            _listenerMain = listenerMain;

            LoadListenedHistory();
        }

        public void LoadListenedHistory()
        {
            if (_listenerMain._listeningHistory.Any())
                InfoPanel.Visibility = Visibility.Collapsed;
            else return;

            ListenedSongsList.Children.Clear();

            foreach (var songId in _listenerMain._listeningHistory)
            {
                var song = _context.Songs.Find(songId);
                var card = UIElementsFactory.CreateSongCard(song, songId, _listenerMain.SongCard_Click);

                RemoveTotalPlaysFromCard(card);

                ListenedSongsList.Children.Add(card);
            }

            ListenedSongsListScroll.ScrollToEnd();
        }

        void RemoveTotalPlaysFromCard(Border card)
        {
            if (card?.Child is not Grid grid) return;

            var infoPanel = grid.Children.OfType<TextBlock>().FirstOrDefault();
            grid.Children.Remove(infoPanel);
        }

        private void HideHistoryVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            ListenedHistoryGrid.Visibility = Visibility.Collapsed;
            ShowHistoryButton.Visibility = Visibility.Visible;

            _listenerMain.MainFields.ColumnDefinitions[2].Width = GridLength.Auto;
        }

        private void ShowHistoryVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            ListenedHistoryGrid.Visibility = Visibility.Visible;
            ShowHistoryButton.Visibility = Visibility.Collapsed;

            _listenerMain.MainFields.ColumnDefinitions[2].Width = new GridLength(0.5, GridUnitType.Star);
        }
    }
}
