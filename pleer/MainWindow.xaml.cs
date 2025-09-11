using Microsoft.EntityFrameworkCore;
using pleer.Models;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
        Media _selectedMedia;

        private bool _isSliding = false;

        private DispatcherTimer _progressTimer;

        private PlayerState _playerState = PlayerState.Stopped;
        private enum PlayerState
        {
            Stopped,    // Воспроизведение остановлено
            Playing,    // Идет воспроизведение
            Paused      // На паузе
        }

        public MainWindow()
        {
            InitializeComponent();

            LoadMediaList();
        }

        async void InitilizeTimer()
        {
            _progressTimer = new DispatcherTimer();
            _progressTimer.Tick += new EventHandler(TimerTick);
            _progressTimer.Interval = TimeSpan.FromMilliseconds(150);
            _progressTimer.Start();
        }

        //Create media card
        async void LoadMediaList()
        {
            var medias = await _context.Media.ToArrayAsync();

            if (medias.Count() == 0)
                return;

            foreach (var media in medias)
            {
                var card = CreateMediaCard(media);
                MediaList.Children.Add(card);
            }
        }

        private UIElement CreateMediaCard(Media media)
        {
            Grid grid = new Grid
            {
                Height = 55,
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(media.AlbumCoverPath);
            bitmap.DecodePixelWidth = 90; 
            bitmap.EndInit();

            var image = new Image
            {
                Source = bitmap,
                Stretch = Stretch.UniformToFill,
                Width = 45,
                Height = 45,
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.LowQuality);

            var imageBorder = new Border
            {
                Width = 45,
                Height = 45,
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(20, 5, 5, 5),
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
                Text = media.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 16
            };

            var mediaCreator = new TextBlock
            {
                Text = media.Creator,
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
                Tag = media,
            };

            border.MouseLeftButtonUp += MediaCard_Click;

            return border;
        }

        private void MediaCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Media media)
            {
                SelectMedia(media);
            }
        }


        //Media manage
        void TimerTick(object sender, EventArgs e)
        {
            if (!_isSliding && _mediaPlayer.Position.TotalSeconds >= 0)
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

        void SelectMedia(Media media)
        {
            _mediaPlayer.Close();
            _selectedMedia = media;

            _mediaPlayer.Open(new Uri(_selectedMedia.SongPath));

            _mediaPlayer.Play();
            _playerState = PlayerState.Playing;

            _mediaPlayer.Volume = VolumeSlider.Value;

            InitilizeTimer();

            _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
        }

        void LoadSongMetadata()
        {
            PlayMediaButton.Content = "II";

            SongName.Text = _selectedMedia.Name;
            SingerName.Text = _selectedMedia.Creator;

            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(_selectedMedia.AlbumCoverPath);
            bitmap.DecodePixelWidth = 90;
            bitmap.EndInit();

            if (string.IsNullOrEmpty(_selectedMedia.AlbumCoverPath))
            {
                AlbumCover.Source = new BitmapImage(new Uri("..\\Resources\\ServiceImages\\NoMediaImage.png"));
            }
            else 
                AlbumCover.Source = bitmap;
        }

        private void PlayMedia_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMedia == null)
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

        private void PreviousMedia_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NextMedia_Click(object sender, RoutedEventArgs e)
        {

        }

        // Для перемотки
        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Если пользователь перемещает слайдер (а не программа)
            if (_isSliding && _mediaPlayer != null)
            {
                // Устанавливаем новую позицию для медиаплеера
                _mediaPlayer.Position = TimeSpan.FromSeconds(e.NewValue);

                // Обновляем отображение текущего времени
                currentMediaTime.Text = _mediaPlayer.Position.ToString(@"mm\:ss");
            }
        }

        // Обработчики для определения, когда пользователь начинает и заканчивает перемещение слайдера
        private void ProgressSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isSliding = true;
        }

        private void ProgressSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isSliding = false;
            _mediaPlayer.Position = TimeSpan.FromSeconds(progressSlider.Value);
        }

        //Изменение громкости
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Volume = VolumeSlider.Value;
            }
        }

        //Добавление песни
        private void UploadSongButton_Click(object sender, RoutedEventArgs e)
        {
            var uploadSongWindow = new UploadSongWindow();
            uploadSongWindow.ShowDialog();
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