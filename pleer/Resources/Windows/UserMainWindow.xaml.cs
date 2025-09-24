using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pleer.Models.CONTEXT;
using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Pages;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace pleer.Resources.Windows
{
    public partial class UserMainWindow : Window
    {
        dbContext _context = new dbContext();
        User _user;

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

        public UserMainWindow()
        {
            InitializeComponent();

            _user = _context.Users.Find(1);

            InitilizeData. SeedData();
            PlaylistList.Navigate(new SimplePlaylistsList(this, _user));
        }

        async void InitilizeTimer()
        {
            _progressTimer = new DispatcherTimer();
            _progressTimer.Tick += new EventHandler(TimerTick);
            _progressTimer.Interval = TimeSpan.FromMilliseconds(150);
            _progressTimer.Start();
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
        }

        public void SelectSong(Song song)
        {
            _mediaPlayer.Close();
            _selectedSong = song;

            _mediaPlayer.Open(new Uri(_selectedSong.FilePath));

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

        public void SongCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Song song)
            {
                SelectSong(song);
            }
        }

        async void LoadSongMetadata()
        {
            PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;

            var album = await _context.Albums.FindAsync(_selectedSong.AlbumId);
            var artist = await _context.Artists.FindAsync(album.ArtistId);

            SongName.Text = _selectedSong.Title;
            SingerName.Text = artist.Name;

            var cover = await _context.AlbumCovers.FindAsync(album.AlbumCoverId);

            if (string.IsNullOrEmpty(cover.FilePath))
            {
                AlbumCover.Source = new BitmapImage(new Uri("..\\Resources\\ServiceImages\\NoMediaImage.png"));
            }
            else
                AlbumCover.Source = UIServiceMethods.DecodePhoto(cover.FilePath, 90);
        }

        private void PlayMedia_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer == default)
                return;

            switch (_playerState)
            {
                case PlayerState.Paused:
                    PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                    _mediaPlayer.Play();
                    _playerState = PlayerState.Playing;
                    break;

                case PlayerState.Playing:
                    PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
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
                SelectSong(_selectedSong);
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
                SelectSong(_selectedSong);
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

        private void MutePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeSlider.Value != 0)
            {
                VolumeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
                VolumeSlider.Value = 0;
                _mediaPlayer.Volume = VolumeSlider.Value;
            }
            else
            {
                VolumeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
                VolumeSlider.Value = VolumeSlider.Maximum / 2;
                _mediaPlayer.Volume = VolumeSlider.Value;
            }
        }

        //Добавление песни
        private void LoginAsArtistButton_Click(object sender, RoutedEventArgs e)
        {
            var artistMainWindow = new ArtistMainWindow();
            _mediaPlayer.Close();
            artistMainWindow.Show(); this.Close();
        }

        //Navigate
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateMethods.OpenSongsSimpleList(this, _user);
        }

        //Site Opening
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Close();
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
