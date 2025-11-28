using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Pages.Collections;
using pleer.Resources.Pages.GeneralPages;
using pleer.Resources.Pages.UserPages;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace pleer.Resources.Windows
{
    public partial class ArtistMainWindow : Window
    {
        DBContext _context = new();

        public CollectionsList _collectionMain;

        Artist _artist;

        MediaPlayer _mediaPlayer = new();
        Song _selectedSong;
        public Album _currentAlbum;

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

        public ArtistMainWindow()
        {
            InitializeComponent();

            NonArtistWindow();
        }

        public ArtistMainWindow(Artist artist)
        {
            InitializeComponent();

            _artist = artist;

            LoadArtistData();
        }

        void NonArtistWindow()
        {
            ArtistFunctionsList.IsEnabled = false;
            OperationField.Navigate(new UnauthorizedNoticePage(this));

            _collectionMain = new CollectionsList(this, _artist);
        }

        void LoadArtistData()
        {
            InitializeData.SeedData(_context);

            if (_artist != null)
            {
                _collectionMain = new CollectionsList(this, _artist);

                LoginButton.Visibility = Visibility.Collapsed;
                ProfilePictureEllipse.Visibility = Visibility.Visible;

                ArtistName.Text = _artist.Name;

                var picture = _context.ProfilePictures
                    .Find(_artist.ProfilePictureId);

                if (picture != null)
                    ProfilePictureImage.ImageSource = UIElementsFactory
                        .DecodePhoto(picture.FilePath, (int)ProfilePictureEllipse.Width);
                else
                    ProfilePictureImage.ImageSource = UIElementsFactory
                        .DecodePhoto(InitializeData.GetDefaultProfilePicturePath(), (int)ProfilePictureEllipse.Width);
            }
        }

        // TIMER & SLIDER
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

        // PLAY SONG
        void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            if (_mediaPlayer.NaturalDuration.HasTimeSpan && _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds >= 0)
            {
                totalMediaTime.Text = _mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                progressSlider.Maximum = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            }

            LoadSongMetadata();
        }

        void SelectSong(Song song)
        {
            _mediaPlayer.Close();
            _selectedSong = song;

            MetadataPanel.Children.Remove(VisualBorder);

            _mediaPlayer.Open(new Uri(_selectedSong.FilePath));
            _mediaPlayer.Volume = VolumeSlider.Value;

            _mediaPlayer.Play();
            _playerState = PlayerState.Playing;

            InitilizeTimer();
            InitializeProgressUpdates();

            _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
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

            try
            {
                AlbumCover.ImageSource =
                    UIElementsFactory
                        .DecodePhoto(cover.FilePath, (int)AlbumCoverBorder.ActualWidth) ??
                    UIElementsFactory
                        .DecodePhoto(InitializeData.GetDefaultCoverPath(), (int)AlbumCoverBorder.ActualWidth);
            }
            catch (Exception ex)
            {
                Debug.Write($"Oшибка загрузки фото: {ex.Message}");
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

        public void SongCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Song song)
                SelectSong(song);
        }

        // Перемотка
        private void PreviousMedia_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer == default)
                return;

            ProgressSlider_ChangeValue(0);
        }

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

        // Изменение громкости
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

        // collection card
        public void AlbumCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Album album)
            {
                if (_currentAlbum != album)
                    OperationField.Navigate(new OpenCollection(_collectionMain, this, album));

                _currentAlbum = album;
            }

            if (sender is TextBlock textBlock && textBlock.Tag is Album songAlbum)
            {
                if (_currentAlbum != songAlbum)
                    OperationField.Navigate(new OpenCollection(_collectionMain, this, songAlbum));

                _currentAlbum = songAlbum;
            }
        }

        // КНОПКИ МЕНЮ ИСПОЛНИТЕЛЯ
        private void ArtistReleasesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_artist != null)
                OperationField.Navigate(new CollectionsList(this, _artist));
        }

        private void ArtistProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (_artist != null)
                OperationField.Navigate(new ArtistProfilePage(this, _artist));
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_artist != null)
                OperationField.Navigate(new ArtistStatisticsPage(_artist));
        }

        // ОБЩИЕ ФУНКЦИОНАЛЬНЫЕ КНОПКИ
        private void LoginAsListinerButton_Click(object sender, RoutedEventArgs e)
        {
            new ListenerMainWindow().Show(); Close();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            FullWindow.Navigate(new LoginPage(this));
        }

        private void LoginAsAdminButton_Click(object sender, RoutedEventArgs e)
        {
            new AdminMainWindow().Show(); Close();
        }

        private void ProfileImage_Click(object sender, MouseButtonEventArgs e)
        {
            FullWindow.Navigate(new ProfilePage(this, _artist));
        }

        private void OperationField_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // Этот блок выполнится ТОЛЬКО если текущая страница - CollectionsList.
            if (e.Content is CollectionsList)
                SongFunctionalBar.Visibility = Visibility.Visible;
            else
            {
                _mediaPlayer.Pause();
                SongFunctionalBar.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mediaPlayer.Stop();
        }
    }
}
