using Microsoft.Win32;
using NAudio.Wave;
using pleer.Models.CONTEXT;
using pleer.Models.DB_Models;
using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Pages.Songs;
using pleer.Resources.Windows;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace pleer.Resources.Pages.AlbumsAndPlaylists
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

        private void ChangeAlbumCoverGrid_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

                    if (bitmap.PixelWidth != 150 || bitmap.PixelHeight != 150)
                    {
                        bitmap = UIElementsFactory.ResizeImageTo300x300(_coverPath);
                    }

                    AlbumCoverDemonstrate.Source = bitmap;
                }
                catch (Exception ex)
                {
                    ErrorNoticePanel.Text = "Ошибка загрузки изображения";
                }
            }
        }

        void LoadSongList()
        {
            SongsList.Children.Clear();

            foreach (var song in _songList)
            {
                var card = UIElementsFactory.CreateSongCardArtist(this, _artist, song);
                SongsList.Children.Add(card);
            }

            var addNewSongButton = new Button()
            {
                Content = "Добавить песню",
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = Application.Current.TryFindResource(
                        "SimpleButton") as Style,
            };

            addNewSongButton.Click += AddNewSongButton_Click;

            SongsList.Children.Add(addNewSongButton);
        }

        public void RemoveSongFromAlbum(Song song)
        {
            _album.Songs.Remove(song);
            LoadSongList();
        }

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
                    };

                    using (var audioFile = new AudioFileReader(songPath))
                    {
                        song.TotalDuration = audioFile.TotalTime;
                    }

                    _songList.Add(song);

                    LoadSongList();
                }
                catch (Exception ex)
                {
                    ErrorNoticePanel.Text = "Ошибка загрузки файла песни";
                }
            }
        }

        private async void ReleaseNewAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorNoticePanel.Text = string.Empty;

            if (string.IsNullOrEmpty(AlbumTitle.Text))
            {
                ErrorNoticePanel.Text = "Название альбома не может быть пустым";
                return;
            }

            if (_coverPath == null)
            {
                ErrorNoticePanel.Text = "Ошибка сохранения обложки.";
                return;
            }

            if (!_songList.Any())
            {
                ErrorNoticePanel.Text = "Альбом должен содержать хотябы одну песню.";
                return;
            }

            var result = MessageBox.Show(
                         "Вы уверены, что хотите выпустить альбом? Перед этим убедитесь что все данные заполнены верно",
                         "Подтверждение выпуска нового альбома",
                         MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await ReleaseNewAlbumAsync();
            }
            else
                return;
        }

        async Task ReleaseNewAlbumAsync()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _album.Title = AlbumTitle.Text;
                    _album.ArtistId = _artist.Id;
                    _album.ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow);

                    _context.Add(_album);
                    await _context.SaveChangesAsync();

                    var albumCover = PictureService.SaveAlbumCover(_coverPath, _album.Id);

                    _context.Add(albumCover);
                    await _context.SaveChangesAsync();

                    _album.CoverId = albumCover.Id;

                    foreach (var song in _songList)
                    {
                        song.AlbumId = _album.Id;
                        _context.Add(song);
                    }
                    await _context.SaveChangesAsync();

                    foreach (var song in _songList)
                    {
                        _album.SongsId.Add(song.Id);
                    }
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    MessageBox.Show("Альбом успешно выпущен!", "Выпуск альбома",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    _artistMain.OperationField.Navigate(new AlbumsList(_artistMain, _artist));
                }
                catch (Exception ex)
                {
                    ErrorNoticePanel.Text = "Ошибка выпуска альбома.";
                }
            }
        }
    }
}
