using Microsoft.Win32;
using NAudio.Wave;
using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace pleer.Resources.Pages.Collections
{
    /// <summary>
    /// Логика взаимодействия для LoadSongToDB.xaml
    /// </summary>
    public partial class CreateAlbum : Page
    {
        DBContext _context = new();

        ArtistMainWindow _artistMain;

        Artist _artist;
        Album _album = new();

        List<Song> _songList = [];
        Song _currentSong;
        Border _currentCard;

        string _coverPath;

        public CreateAlbum(ArtistMainWindow main, Artist artist)
        {
            InitializeComponent();

            _artistMain = main;
            _artist = artist;

            CoverMouseEvents();
            LoadSongList();
        }

        void CoverMouseEvents()
        {
            AlbumCoverGrid.MouseEnter += (s, e) => ChangeAlbumCoverGrid.Visibility = Visibility.Visible;
            AlbumCoverGrid.MouseLeave += (s, e) => ChangeAlbumCoverGrid.Visibility = Visibility.Collapsed;
        }

        // ALBUM cover
        private void ChangeAlbumCoverGrid_Click(object sender, MouseButtonEventArgs e)
        {
            ErrorNoticePanel.Text = string.Empty;

            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите обложку альбома",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp|Все файлы|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _coverPath = openFileDialog.FileName;
                    var bitmap = new BitmapImage(new Uri(_coverPath));

                    if (bitmap.PixelWidth != 300 || bitmap.PixelHeight != 300)
                    {
                        bitmap = UIElementsFactory.ResizeImageTo300x300(_coverPath);
                    }

                    AlbumCoverDemonstrate.ImageSource = bitmap;
                }
                catch (Exception ex)
                {
                    ErrorNoticePanel.Text = "Ошибка загрузки изображения";
                }
            }
        }

        //SONGS LIST
        void LoadSongList()
        {
            SongsList.Children.Clear();
            SongsList.Children.Remove(AddSongButton);

            try
            {
                foreach (var song in _songList)
                {
                    if (song != null)
                    {
                        int songIndex = _songList.IndexOf(song);

                        var card = UIElementsFactory.CreateSongCard(song, songIndex, this, SongCard_Click);
                        SongsList.Children.Add(card);

                        if (_currentCard != null)
                            CardSelected(card);
                    }
                }
            }
            catch { }

            AddSongButton = new Button()
            {
                Content = "Добавить песню",
                Padding = new Thickness(10),
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = Application.Current.TryFindResource(
                        "SimpleButton") as Style,
            };

            AddSongButton.Click += AddNewSongButton_Click;

            SongsList.Children.Add(AddSongButton);
        }

        // RENAME & CARD click
        private void SongCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card)
            {
                CardSelected(card);
            }
        }

        void CardSelected(Border card)
        {
            if (_currentCard != null)
                UIElementsFactory.SetCardTitleColor(_currentCard, "#eeeeee");

            UIElementsFactory.SetCardTitleColor(card, "#90ee90");

            _currentCard = card;

            if (card.Tag is Song song)
            {
                _currentSong = song;
                _currentCard = card;

                RenameSongButton.IsEnabled = true;
                SongRenameTextBox.Text = _currentSong.Title;
            }
        }

        public void RemoveSongFromAlbum(Song song)
        {
            RenameSongButton.IsEnabled = false;

            _songList.Remove(song);
            LoadSongList();
        }

        private void RenameSongButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSong == null) return;

            if (string.IsNullOrEmpty(SongRenameTextBox.Text))
            {
                ErrorNoticePanel.Text = "Название песни не может быть пустым.";
                return;
            }

            _songList.Find(s => s.FilePath == _currentSong.FilePath).Title = SongRenameTextBox.Text;
            LoadSongList();
        }

        // NEW song
        private void AddNewSongButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorNoticePanel.Text = string.Empty;

            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите MP3 файл",
                Filter = "MP3 файлы|*.mp3|Все файлы|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string songPath = openFileDialog.FileName;

                    var song = new Song()
                    {
                        Title = Path.GetFileNameWithoutExtension(songPath),
                        FilePath = songPath,
                        TotalDuration = new AudioFileReader(songPath).TotalTime
                    };

                    _songList.Add(song);
                    _currentSong = song;

                    if (string.IsNullOrEmpty(song.Title))
                        song.Title = $"Песня {_songList.IndexOf(song) + 1}";

                    LoadSongList();
                }
                catch (Exception ex)
                {
                    ErrorNoticePanel.Text = "Ошибка загрузки файла песни";
                }
            }
        }

        // RELEASE album
        private async void ReleaseNewAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorNoticePanel.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(AlbumTitle.Text))
            {
                ErrorNoticePanel.Text = "Название альбома не может быть пустым";
                return;
            }

            if (string.IsNullOrEmpty(_coverPath) || !File.Exists(_coverPath))
            {
                ErrorNoticePanel.Text = "Ошибка: файл обложки не найден.";
                return;
            }

            if (_songList == null || !_songList.Any())
            {
                ErrorNoticePanel.Text = "Альбом должен содержать хотя бы одну песню.";
                return;
            }

            var result = MessageBox.Show(
                "Вы уверены, что хотите выпустить альбом? Перед этим убедитесь, что все данные заполнены верно.",
                "Подтверждение выпуска нового альбома",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await ReleaseNewAlbumAsync();
            }
        }

        private async Task ReleaseNewAlbumAsync()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _album.Title = AlbumTitle.Text.Trim();
                    _album.CreatorId = _artist.Id;
                    _album.ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow);

                    _context.Albums.Add(_album);
                    await _context.SaveChangesAsync();

                    var albumCover = PictureService.SaveAlbumCover(_coverPath, _album.Id);
                    _context.AlbumCovers.Add(albumCover);
                    await _context.SaveChangesAsync();

                    _album.CoverId = albumCover.Id;

                    foreach (var song in _songList)
                    {
                        song.AlbumId = _album.Id;
                        _context.Songs.Add(song);
                    }
                    await _context.SaveChangesAsync();

                    foreach (var song in _songList)
                    {
                        _album.SongsId.Add(song.Id);
                    }
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    MessageBox.Show( "Альбом успешно выпущен!", "Выпуск альбома",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    _artistMain.OperationField.Navigate(new CollectionsList(_artistMain, _artist));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    ErrorNoticePanel.Text = $"Ошибка выпуска альбома: {ex.Message}";
                }
            }
        }
    }
}
