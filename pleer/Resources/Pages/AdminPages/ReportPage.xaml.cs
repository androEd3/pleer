using pleer.Models.DatabaseContext;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;

namespace pleer.Resources.Pages.AdminPages
{
    /// <summary>
    /// Логика взаимодействия для ReportPage.xaml
    /// </summary>
    public partial class ReportPage : Page
    {
        private DBContext _context = new();

        public ReportPage()
        {
            InitializeComponent();

            Loaded += (s, e) => LoadReportData();
        }

        private void LoadReportData()
        {
            DateOnly? startDate = StartDateSelector?.SelectedDate.HasValue == true
                ? DateOnly.FromDateTime(StartDateSelector.SelectedDate.Value)
                : null;

            DateOnly? endDate = EndDateSelector?.SelectedDate.HasValue == true
                ? DateOnly.FromDateTime(EndDateSelector.SelectedDate.Value)
                : null;

            var listeners = FilterByDate(_context.Listeners, l => l.CreatedAt, startDate, endDate).ToList();
            var artists = FilterByDate(_context.Artists, a => a.CreatedAt, startDate, endDate).ToList();
            var albums = FilterByDate(_context.Albums, a => a.ReleaseDate, startDate, endDate).ToList();
            var playlists = FilterByDate(_context.Playlists, p => p.CreatedAt, startDate, endDate).ToList();
            var songs = FilterByDate(_context.Songs, s => s.Album.ReleaseDate, startDate, endDate).ToList();

            TotalUsers.Text = $"{listeners.Count + artists.Count} ({artists.Count} + {listeners.Count})";
            TotalAlbums.Text = albums.Count.ToString();
            TotalPlaylists.Text = playlists.Count.ToString();
            TotalSongs.Text = songs.Count.ToString();

            var mostPopularSong = songs
                .OrderByDescending(s => s.TotalPlays)
                .FirstOrDefault();
            MostPopularSong.Text = mostPopularSong?.Title ?? "Нет данных";

            var mostPopularAlbum = albums
                .OrderByDescending(a => a.TotalPlays)
                .FirstOrDefault();
            MostPopularAlbum.Text = mostPopularAlbum?.Title ?? "Нет данных";
        }

        // Универсальный метод фильтрации по датам
        private IQueryable<T> FilterByDate<T>(
            IQueryable<T> query,
            Expression<Func<T, DateOnly>> dateSelector,
            DateOnly? startDate,
            DateOnly? endDate)
        {
            if (startDate.HasValue)
            {
                var start = startDate.Value;
                var parameter = dateSelector.Parameters[0];
                var body = System.Linq.Expressions.Expression.GreaterThanOrEqual(dateSelector.Body, System.Linq.Expressions.Expression.Constant(start));
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, parameter);
                query = query.Where(lambda);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value;
                var parameter = dateSelector.Parameters[0];
                var body = System.Linq.Expressions.Expression.LessThanOrEqual(dateSelector.Body, System.Linq.Expressions.Expression.Constant(end));
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, parameter);
                query = query.Where(lambda);
            }

            return query;
        }

        private void DateSelector_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadReportData();
        }

        private void UpdateReportDataButton_Click(object sender, RoutedEventArgs e)
        {
            LoadReportData();
        }
    }
}
