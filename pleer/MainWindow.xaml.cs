using Microsoft.EntityFrameworkCore;
using pleer.Models.CONTEXT;
using pleer.Models.Media;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace pleer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        dbContext _context = new dbContext();

        MediaPlayer _mediaPlayer = new MediaPlayer();
        Song _selectedSong;

        List<Song> _listeningHistory = new List<Song>();
        int _songSerialNumber;

        private bool _isDraggingMediaSlider = false;
        private bool _isDraggingVolumeSlider = false;

        private bool _isUnpressedMediaSlider = true;

        private DispatcherTimer _progressTimer;

        private PlayerState _playerState = PlayerState.Paused;
        private enum PlayerState
        {
            Playing,    // Идет воспроизведение
            Paused      // На паузе
        }

        public MainWindow()
        {
            InitializeComponent();

            //InitilizeData.SeedData();

            LoadSongsList();
        }

        async void InitilizeTimer()
        {
            _progressTimer = new DispatcherTimer();
            _progressTimer.Tick += new EventHandler(TimerTick);
            _progressTimer.Interval = TimeSpan.FromMilliseconds(150);
            _progressTimer.Start();
        }

        //Create media card
        async public void LoadSongsList()
        {
            SongsList.Children.Clear();

            var songs = await _context.Songs.ToArrayAsync();

            if (songs.Count() == 0)
                return;

            foreach (var song in songs)
            {
                var card = CreateMediaCard(song);
                SongsList.Children.Add(card);
            }
        }

        private string LoadAlbumCover(int Id)
        {
            return _context.AlbumCovers.FirstOrDefault(d => d.Id == Id).AlbumCoverPath;
        }

        private UIElement CreateMediaCard(Song song)
        {
            Grid grid = new Grid
            {
                Height = 55,
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(LoadAlbumCover(song.Id));
            bitmap.DecodePixelWidth = 90; 
            bitmap.EndInit();

            var image = new Image
            {
                Source = bitmap,
                Style = Application.Current.TryFindResource("SongCover") as Style
            };

            var imageBorder = new Border
            {
                Width = 45,
                Height = 45,
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(15, 5, 5, 5),
                Child = image,
            };

            Grid.SetColumn(imageBorder, 0);
            grid.Children.Add(imageBorder);

            var infoPanel = new StackPanel
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
            };

            var mediaName = new TextBlock
            {
                Text = song.Title,
                FontWeight = FontWeights.Bold,
                FontSize = 16
            };

            var mediaCreator = new TextBlock
            {
                Text = song.Artist,
                Foreground = Brushes.Gray,
                FontSize = 14
            };

            infoPanel.Children.Add(mediaName);
            infoPanel.Children.Add(mediaCreator);

            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            Border border = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4F4F4")),

                Margin = new Thickness(5),
                Cursor = Cursors.Hand,

                Child = grid,
                Tag = song,
            };

            border.MouseLeftButtonUp += MediaCard_Click;

            return border;
        }

        private void MediaCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Song media)
            {
                SelectMedia(media);
            }
        }


        //Media manage
        void TimerTick(object sender, EventArgs e)
        {
            if (!_isDraggingMediaSlider && _mediaPlayer.Position.TotalSeconds >= 0)
            {
                currentMediaTime.Text = _mediaPlayer.Position.ToString(@"mm\:ss");
                progressSlider.Value = _mediaPlayer.Position.TotalSeconds;
            }

            _mediaPlayer.MediaEnded += (s, e) =>
            {
                _progressTimer.Stop();
            };
        }

        void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            if (_mediaPlayer.NaturalDuration.HasTimeSpan && _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds >= 0)
            {
                totalMediaTime.Text = _mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                progressSlider.Maximum = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            }

            LoadSongMetadata();
        }

        void AddSongToHistory(Song song)
        {
            _listeningHistory.Add(song);

            _songSerialNumber = _listeningHistory.Count();

            ListeningHistoryList.Text += song.Id.ToString() + " ";
        }

        void SelectMedia(Song song)
        {
            _mediaPlayer.Close();
            _selectedSong = song;

            _mediaPlayer.Open(new Uri(_selectedSong.SongPath));

            _mediaPlayer.Play();
            _playerState = PlayerState.Playing;

            if (_listeningHistory.Count() == 0)
            {
                AddSongToHistory(song);
            }
            if (_listeningHistory[_songSerialNumber - 1].Id != song.Id)
            {
                AddSongToHistory(song);
            }

            _mediaPlayer.Volume = VolumeSlider.Value;

            InitilizeTimer();

            _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
        }

        void LoadSongMetadata()
        {
            PlayMediaButton.Content = "II";

            SongName.Text = _selectedSong.Title;
            SingerName.Text = _selectedSong.Artist;

            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(LoadAlbumCover(_selectedSong.Id));
            bitmap.DecodePixelWidth = 90;
            bitmap.EndInit();

            if (string.IsNullOrEmpty(LoadAlbumCover(_selectedSong.Id)))
            {
                AlbumCover.Source = new BitmapImage(new Uri("..\\Resources\\ServiceImages\\NoMediaImage.png"));
            }
            else
                AlbumCover.Source = bitmap;
        }

        private void PlayMedia_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer == default)
                return;

            switch (_playerState)
            {
                case PlayerState.Paused:
                    PlayMediaButton.Content = "II";
                    _mediaPlayer.Play();
                    _playerState = PlayerState.Playing;
                    break;

                case PlayerState.Playing:
                    PlayMediaButton.Content = "▶";
                    _mediaPlayer.Pause();
                    _playerState = PlayerState.Paused;
                    break;
            }
        }

        //СКИП
        private void PreviousMedia_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer == default)
                return;

            if (_mediaPlayer.Position < TimeSpan.FromSeconds(3) && _songSerialNumber - 1 > 0)
            {
                _songSerialNumber -= 1;
                _selectedSong = _listeningHistory[_songSerialNumber - 1];
                SelectMedia(_selectedSong);
            }
            else
                ProgressSlider_ChangeValue(0);
        }

        private void NextMedia_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer == default)
                return;

            if (_songSerialNumber < _listeningHistory.Count())
            {
                ProgressSlider_ChangeValue(0);

                _songSerialNumber += 1;

                _selectedSong = _listeningHistory[_songSerialNumber - 1];
                SelectMedia(_selectedSong);
            }
            // тут можно сделать скип на другие плейлисты потому что текущий все гг закончился
        }

        // Для перемотки песни
        void ProgressSlider_ChangeValue(double value)
        {
            _mediaPlayer.Position = TimeSpan.FromSeconds(value);
            currentMediaTime.Text = _mediaPlayer.Position.ToString(@"mm\:ss");
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isDraggingMediaSlider && _isUnpressedMediaSlider && _mediaPlayer != null)
            {
                ProgressSlider_ChangeValue(e.NewValue);
            }
        }

        private void ProgressSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingMediaSlider = true;
            _isUnpressedMediaSlider = false;
        }

        private void ProgressSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingMediaSlider = false;
            _isUnpressedMediaSlider = true;
            _mediaPlayer.Position = TimeSpan.FromSeconds(progressSlider.Value);
        }

        //Изменение громкости
        private void VolumeSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingVolumeSlider = true;
        }

        private void VolumeSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingVolumeSlider)
            {
                _isDraggingVolumeSlider = false;
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mediaPlayer != null && _isDraggingVolumeSlider)
            {
                _mediaPlayer.Volume = VolumeSlider.Value;
            }
        }

        //Добавление песни
        private void UploadSongButton_Click(object sender, RoutedEventArgs e)
        {
            var uploadSongWindow = new UploadSongWindow();
            uploadSongWindow.ShowDialog();
            LoadSongsList();
        }

        //Site Opening
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLoginBrowser();
        }
        void OpenLoginBrowser()
        {
            _context = new dbContext();

            try
            {
                _context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            var authUrl = "https://localhost:7021/Home/Index";
            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });
        }
    }
}