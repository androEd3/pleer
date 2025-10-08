using Microsoft.EntityFrameworkCore;
using pleer.Models.CONTEXT;
using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace pleer.Resources.Windows
{
    public partial class ListenerMainWindow : Window
    {
        DBContext _context = new();
        Listener _listener;

        MediaPlayer _mediaPlayer = new();
        Song _selectedSong;

        List<Song> _listeningHistory = [];
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

        public ListenerMainWindow()
        {
            InitializeComponent();

            LoadUserData(_listener);
        }

        public ListenerMainWindow(Listener listener)
        {
            InitializeComponent();

            _listener = listener;

            LoadUserData(_listener);
        }

        void LoadUserData(Listener listener)
        {
            InitilizeData.SeedData(_context);

            if (listener != null)
            {

                PlaylistList.Navigate(new SimplePlaylistsList(this, listener));
                NoticePanel.Visibility = Visibility.Collapsed;
            }
            else
                NoticePanel.Visibility = Visibility.Visible;
        }

        void InitilizeTimer()
        {
            _progressTimer = new DispatcherTimer();
            _progressTimer.Tick += OnCompositionRendering;
            _progressTimer.Interval = TimeSpan.FromMilliseconds(33);
            _progressTimer.Start();
        }

        //Media manage
        void InitializeProgressUpdates()
        {
            CompositionTarget.Rendering += OnCompositionRendering;
        }

        void OnCompositionRendering(object sender, EventArgs e)
        {
            if (!_isDraggingMediaSlider && _mediaPlayer.Position.TotalSeconds >= 0)
            {
                var position = _mediaPlayer.Position;
                currentMediaTime.Text = position.ToString(@"mm\:ss");
                progressSlider.Value = position.TotalSeconds;
            }
            else
                StopProgressUpdates();
        }

        void StopProgressUpdates()
        {
            CompositionTarget.Rendering -= OnCompositionRendering;
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
            _songSerialNumber = _listeningHistory.Count;
        }

        void SelectSong(Song song)
        {
            _mediaPlayer.Close();
            _selectedSong = song;

            _mediaPlayer.Open(new Uri(_selectedSong.FilePath));
            _mediaPlayer.Volume = VolumeSlider.Value;

            _mediaPlayer.Play();
            _playerState = PlayerState.Playing;

            InitilizeTimer();
            InitializeProgressUpdates();

            if (_listeningHistory.Count == 0)
                AddSongToHistory(song);
            if (_listeningHistory[_songSerialNumber - 1].Id != song.Id)
                AddSongToHistory(song);

            _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
        }

        public void SongCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Song song)
                SelectSong(song);
        }

        async Task LoadSongMetadata()
        {
            PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;

            var album = await _context.Albums
                .FindAsync(_selectedSong.AlbumId);
            var artist = await _context.Artists
                .FindAsync(album.ArtistId);
            var cover = await _context.AlbumCovers
                .FindAsync(album.CoverId);

            SongName.Text = _selectedSong.Title;
            SingerName.Text = artist.Name;

            if (cover != null)
                AlbumCover.Source = UIServiceMethods.DecodePhoto(cover.FilePath, 90);
            else
            {
                var defaultCover = await _context.AlbumCovers.FindAsync(1);
                AlbumCover.Source = UIServiceMethods.DecodePhoto(defaultCover.FilePath, 90);
            }
        }

        private void PlayMedia_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer == default)
                return;

            switch (_playerState)
            {
                case PlayerState.Paused:
                    _mediaPlayer.Play();
                    _playerState = PlayerState.Playing;
                    PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                    break;

                case PlayerState.Playing:
                    _mediaPlayer.Pause();
                    _playerState = PlayerState.Paused;
                    PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
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

            if (_songSerialNumber < _listeningHistory.Count)
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
            _mediaPlayer.Close();
            new ArtistMainWindow().Show(); this.Close();
        }

        //Navigate
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateMethods.OpenSongsSimpleList(this, _listener);
        }

        //Login
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Close();
            NavigateMethods.OpenListenerLoginPage(this);
        }
    }
}
