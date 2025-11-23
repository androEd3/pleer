using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Pages.Collections;
using pleer.Resources.Pages.GeneralPages;
using pleer.Resources.Pages.Songs;
using pleer.Resources.Pages.UserPages;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NAudio.Wave;

namespace pleer.Resources.Windows
{
    public partial class ListenerMainWindow : Window
    {
        DBContext _context = new();
        Listener _listener;

        HomePage _homePage;

        IWavePlayer _wavePlayer;
        AudioFileReader _audioFile;

        Song _selectedSong;

        List<int> _listeningHistory = [];
        int _songSerialNumber;

        bool _isDraggingMediaSlider = false;
        bool _isDraggingVolumeSlider;
        bool _isUnpressedMediaSlider = true;

        bool _isListened;
        TimeSpan _listenDuration;

        double _timeTick = 0.016; // ~60 FPS

        PlayerState _playerState = PlayerState.Paused;
        private enum PlayerState
        {
            Playing,
            Paused
        }

        public ListenerMainWindow()
        {
            InitializeComponent();
            InitializeNAudio();
            LoadNonUserWindow();
        }

        public ListenerMainWindow(Listener listener)
        {
            InitializeComponent();

            _listener = listener;

            InitializeNAudio();
            LoadListenerData();
        }

        // Инициализация NAudio
        void InitializeNAudio()
        {
            _wavePlayer = new WaveOutEvent();

            _wavePlayer.PlaybackStopped += WavePlayer_PlaybackStopped;

            InitializeProgressUpdates();
        }

        void LoadNonUserWindow()
        {
            InitializeData.SeedData(_context);

            _homePage = new HomePage(this, _listener);

            MediaLibrary.Navigate(new UnauthorizedNoticePage(this));
            CenterField.Navigate(_homePage);
        }

        void LoadListenerData()
        {
            StopPlayback();

            _homePage = new HomePage(this, _listener);

            CenterField.Navigate(_homePage);

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

        // Плавный прогресс через CompositionTarget.Rendering (~60 FPS)
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
            if (_audioFile != null && !_isDraggingMediaSlider)
            {
                var position = _audioFile.CurrentTime;

                currentMediaTime.Text = position.ToString(@"mm\:ss");
                PositionSlider.Value = position.TotalSeconds;

                if (_playerState == PlayerState.Playing)
                {
                    _listenDuration += TimeSpan.FromSeconds(_timeTick);
                    PlaysCount();
                }
            }
        }

        void PlaysCount()
        {
            if (_isListened || _selectedSong == null)
                return;

            double songTotalDuration = _selectedSong.TotalDuration.TotalSeconds;

            bool shouldCount;

            if (songTotalDuration < 10)
                shouldCount = _listenDuration.TotalSeconds >= songTotalDuration / 2;
            else
                shouldCount = _listenDuration.TotalSeconds >= 10;

            if (shouldCount)
            {
                var song = _context.Songs.Find(_selectedSong.Id);
                var album = _context.Albums.Find(_selectedSong.AlbumId);

                album.TotalPlays++;
                song.TotalPlays++;
                _context.SaveChanges();

                _isListened = true;
                Debug.WriteLine($"Song played! Total plays: {song.TotalPlays}");
            }
        }

        // Событие окончания воспроизведения
        void WavePlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {

        }

        // Воспроизведение песни
        void SelectSong(Song song)
        {
            try
            {
                StopPlayback();

                _selectedSong = song;

                _audioFile = new AudioFileReader(_selectedSong.FilePath);

                _audioFile.Volume = (float)VolumeSlider.Value;

                _wavePlayer.Init(_audioFile);

                _wavePlayer.Play();
                _playerState = PlayerState.Playing;

                LoadSongMetadata();

                if (_listeningHistory.Count == 0)
                    AddSongToHistory(song.Id);
                else if (_listeningHistory[_songSerialNumber - 1] != song.Id)
                    AddSongToHistory(song.Id);

                _isListened = false;
                _listenDuration = TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить файл: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SongCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Song song)
            {
                _homePage.CardPlaying(border);
                SelectSong(song);
            }
        }

        void LoadSongMetadata()
        {
            if (_audioFile == null || _selectedSong == null)
                return;

            PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;

            totalMediaTime.Text = _audioFile.TotalTime.ToString(@"mm\:ss");
            PositionSlider.Maximum = _audioFile.TotalTime.TotalSeconds;

            var album = _context.Albums.Find(_selectedSong.AlbumId);
            var artist = _context.Artists.Find(album.CreatorId);
            var cover = _context.AlbumCovers.Find(album.CoverId);

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
            if (_wavePlayer == null || _audioFile == null)
                return;

            switch (_playerState)
            {
                case PlayerState.Paused:
                    _wavePlayer.Play();
                    _playerState = PlayerState.Playing;
                    PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                    break;

                case PlayerState.Playing:
                    _wavePlayer.Pause();
                    _playerState = PlayerState.Paused;
                    PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                    break;
            }
        }

        //Collection card click
        public void AlbumCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Album album)
            {
                CenterField.Navigate(new OpenCollection(this, album, _listener));
            }
        }

        // ПОИСК
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

        // СКИП
        private void PreviousMedia_Click(object sender, RoutedEventArgs e)
        {
            if (_audioFile == null)
                return;

            if (_audioFile.CurrentTime < TimeSpan.FromSeconds(3) && _songSerialNumber - 1 > 0)
            {
                _songSerialNumber -= 1;

                var song = _context.Songs
                    .Find(_listeningHistory[_songSerialNumber - 1]);
                if (song != default)
                    _selectedSong = song;

                SelectSong(_selectedSong);
            }
            else
            {
                _isListened = false;
                _listenDuration = TimeSpan.Zero;
                PositionSlider_ChangeValue(0);
            }
        }

        private void NextMedia_Click(object sender, RoutedEventArgs e)
        {
            if (_audioFile == null)
                return;

            if (_songSerialNumber < _listeningHistory.Count)
            {
                _songSerialNumber += 1;

                var song = _context.Songs
                    .Find(_listeningHistory[_songSerialNumber - 1]);
                if (song != default)
                    _selectedSong = song;

                SelectSong(_selectedSong);
            }
            // тут можно сделать скип на другие плейлисты
        }

        void AddSongToHistory(int songId)
        {
            _listeningHistory.Add(songId);
            _songSerialNumber = _listeningHistory.Count;
        }

        // Для перемотки песни
        void PositionSlider_ChangeValue(double value)
        {
            if (_audioFile != null)
            {
                _audioFile.CurrentTime = TimeSpan.FromSeconds(value);
                currentMediaTime.Text = _audioFile.CurrentTime.ToString(@"mm\:ss");
            }
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isDraggingMediaSlider && _isUnpressedMediaSlider && _audioFile != null)
            {
                PositionSlider_ChangeValue(e.NewValue);
            }
        }

        private void PositionSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingMediaSlider = true;
            _isUnpressedMediaSlider = false;
        }

        private void PositionSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingMediaSlider = false;
            _isUnpressedMediaSlider = true;

            if (_audioFile != null)
            {
                _audioFile.CurrentTime = TimeSpan.FromSeconds(PositionSlider.Value);
            }
        }

        // Изменение громкости
        private void VolumeSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingVolumeSlider = true;
        }

        private void VolumeSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingVolumeSlider = false;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_audioFile != null)
            {
                _audioFile.Volume = (float)e.NewValue;

                UpdateVolumeIcon(e.NewValue);
            }
        }

        private void MutePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeSlider.Value != 0)
                VolumeSlider.Value = 0;
            else
                VolumeSlider.Value = (float)VolumeSlider.Maximum / 2;
        }

        private void UpdateVolumeIcon(double volume)
        {
            if (volume == 0)
                VolumeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
            else if (volume < 0.5)
                VolumeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeLow;
            else
                VolumeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
        }

        // Остановка воспроизведения
        void StopPlayback()
        {
            _wavePlayer?.Stop();
            _audioFile?.Dispose();
            _audioFile = null;

            _playerState = PlayerState.Paused;
            PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
        }

        // Войти как исполнитель
        private void LoginAsArtistButton_Click(object sender, RoutedEventArgs e)
        {
            CleanupResources();
            new ArtistMainWindow().Show(); Close();
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
            CleanupResources();
        }

        // Освобождение ресурсов
        void CleanupResources()
        {
            StopProgressUpdates();

            _wavePlayer?.Stop();
            _wavePlayer?.Dispose();
            _audioFile?.Dispose();

            _context?.Dispose();
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