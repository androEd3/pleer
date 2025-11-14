using NAudio.Wave;
using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Pages.Collections;
using pleer.Resources.Pages.GeneralPages;
using pleer.Resources.Pages.Songs;
using pleer.Resources.Pages.UserPages;
using System.ComponentModel;
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

        bool _isDraggingMediaSlider = false;
        bool _isDraggingVolumeSlider = false;
        bool _isUnpressedMediaSlider = true;

        DispatcherTimer _progressTimer;

        PlayerState _playerState = PlayerState.Paused;
        private enum PlayerState
        {
            Playing,    // Идет воспроизведение
            Paused      // На паузе
        }

        public ListenerMainWindow()
        {
            InitializeComponent();

            LoadNonUserWindow();
        }

        public ListenerMainWindow(Listener listener)
        {
            InitializeComponent();

            _listener = listener;

            LoadListenerData();
        }

        void LoadNonUserWindow()
        {
            InitializeData.SeedData(_context);

            MediaLibrary.Navigate(new UnauthorizedNoticePage(this));
            CenterField.Navigate(new HomePage(this, _listener));
        }

        void LoadListenerData()
        {
            _mediaPlayer.Stop();

            CenterField.Navigate(new HomePage(this, _listener));

            if (_listener != null)
            {
                LoginButton.Visibility = Visibility.Collapsed;
                ProfilePictureEllipse.Visibility = Visibility.Visible;

                ListenerName.Text = _listener.Name;

                var picture = _context.ProfilePictures
                    .Find(_listener.ProfilePictureId);

                if (picture != null)
                    ProfilePictureImage.ImageSource = UIElementsFactory
                        .DecodePhoto(picture.FilePath, (int)ProfilePictureEllipse.Width);
                else
                    ProfilePictureImage.ImageSource = UIElementsFactory
                        .DecodePhoto(InitializeData.GetDefaultProfilePicturePath(), (int)ProfilePictureEllipse.Width);

                MediaLibrary.Content = null;
                MediaLibrary.Navigate(new MediaLibrary(this, _listener));
            }
        }

        // Таймер
        void InitilizeTimer()
        {
            _progressTimer = new DispatcherTimer();
            _progressTimer.Tick += OnCompositionRendering;
            _progressTimer.Interval = TimeSpan.FromMilliseconds(33);
            _progressTimer.Start();
        }

        // Прогресбар песни
        void InitializeProgressUpdates()
        {
            CompositionTarget.Rendering += OnCompositionRendering;
        }
        void StopProgressUpdates()
        {
            CompositionTarget.Rendering -= OnCompositionRendering;
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

        // Воспроизведение песни
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

        void LoadSongMetadata()
        {
            PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;

            var album = _context.Albums
                .Find(_selectedSong.AlbumId);
            var artist = _context.Artists
                .Find(album.CreatorId);
            var cover = _context.AlbumCovers
                .Find(album.CoverId);

            SongName.Text = _selectedSong.Title;
            SingerName.Text = artist.Name;

            if (cover != null)
                AlbumCover.Source = UIElementsFactory.DecodePhoto(cover.FilePath, (int)AlbumCover.ActualWidth);
            else
            {
                var defaultCover = _context.AlbumCovers
                    .FirstOrDefault(a => a.FilePath == InitializeData.GetDefaultCoverPath());
                AlbumCover.Source = UIElementsFactory.DecodePhoto(defaultCover.FilePath, (int)AlbumCover.ActualWidth);
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

        //Collection Click
        public void PlaylistCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Playlist playlist)
            {
                CenterField.Navigate(new OpenCollection(this, playlist, _listener));
            }
        }

        public void AlbumCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Album album)
            {
                CenterField.Navigate(new OpenCollection(this, album, _listener));
            }
        }

        //ПОИСК
        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string currentSearchText = SearchBar.Text;

            if (string.IsNullOrEmpty(currentSearchText))
                CenterField.Navigate(new HomePage(this, _listener));
            else
                CenterField.Navigate(new SearchResult(this, _listener, currentSearchText));
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            CenterField.Navigate(new HomePage(this, _listener));
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

        // Войти как исполнитель
        private void LoginAsArtistButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Close();
            new ArtistMainWindow().Show(); this.Close();
        }

        // Открытие Профиля
        private void ProfileImage_Click(object sender, MouseButtonEventArgs e)
        {
            FullWindow.Navigate(new ProfilePage(this, _listener));

        }

        // Авторизация
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            FullWindow.Navigate(new LoginPage(this));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _mediaPlayer.Close();
        }

        // ПЕРЕМЕЩЕНИЕ СТРАНИЦ В ЦЕНТРАЛЬНОМ ОКНЕ
        private void BackPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CenterField.CanGoBack)
                CenterField.GoBack();
        }

        private void ForwardPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CenterField.CanGoForward)
                CenterField.GoForward();
        }

        private void UpdateButtonState()
        {
            BackButton.IsEnabled = CenterField.CanGoBack;
            ForwardButton.IsEnabled = CenterField.CanGoForward;
        }

        private void CenterField_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            UpdateButtonState();
        }
    }
}
