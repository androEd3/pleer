using Microsoft.Win32;
using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Service;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace pleer.Resources.Windows
{
    /// <summary>
    /// Логика взаимодействия для PlaylistEditWindow.xaml
    /// </summary>
    public partial class PlaylistEditWindow : Window
    {
        DBContext _context = new();

        Playlist _playlist;
        Album _album;

        string _coverPath;

        public PlaylistEditWindow(Playlist playlist)
        {
            InitializeComponent();

            _playlist = playlist;

            CoverMouseEvents();
            LoadPlaylistData();

            DescriptionPanel.Visibility = Visibility.Visible;
        }

        public PlaylistEditWindow(Album album)
        {
            InitializeComponent();

            _album = album;

            CoverMouseEvents();
            LoadAlbumData();
        }

        void CoverMouseEvents()
        {
            AlbumCoverGrid.MouseEnter += (s, e) => ChangeAlbumCoverGrid.Visibility = Visibility.Visible;
            AlbumCoverGrid.MouseLeave += (s, e) => ChangeAlbumCoverGrid.Visibility = Visibility.Collapsed;
        }

        void LoadPlaylistData()
        {
            AlbumTitle.Text = _playlist.Title;
            PlaylistDescription.Text = _playlist.Description;

            var cover = _context.PlaylistCovers
               .Find(_playlist.CoverId);
            LoadCover(cover.FilePath);
        }

        void LoadAlbumData()
        {
            AlbumTitle.Text = _album.Title;

            var cover = _context.AlbumCovers
               .Find(_album.CoverId);
            LoadCover(cover.FilePath);
        }

        void LoadCover(string filePath)
        {
            AlbumCoverDemonstrate.ImageSource =
                UIElementsFactory
                    .DecodePhoto(filePath, (int)AlbumCoverDemonstrate.ImageSource.Width) ??
                UIElementsFactory
                    .DecodePhoto(InitializeData.GetDefaultCoverPath(), (int)AlbumCoverDemonstrate.ImageSource.Width);
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

        private async void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorNoticePanel.Text = string.Empty;

            if (string.IsNullOrEmpty(AlbumTitle.Text))
            {
                ErrorNoticePanel.Text = "Название не может быть пустым";
                return;
            }

            if (_playlist != null)
                await UpdatePlaylistAsync();
            else
                await UpdateAlbumAsync();
        }

        async Task UpdatePlaylistAsync()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var playlist = _context.Playlists.Find(_playlist.Id);

                    playlist.Title = AlbumTitle.Text;
                    playlist.Description = PlaylistDescription.Text;

                    if (_coverPath != null)
                    {
                        int coverId = _context.PlaylistCovers.Max(pc => pc.Id) + 1;
                        var playlistCover = PictureService.SavePlaylistCover(_coverPath, coverId);

                        _context.Add(playlistCover);
                        await _context.SaveChangesAsync();

                        playlist.CoverId = playlistCover.Id;
                    }
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    Debug.WriteLine("Успешное обновление");

                    Close();
                }
                catch (Exception ex)
                {
                    ErrorNoticePanel.Text = "Ошибка изменения плейлиста.";
                }
            }
        }

        async Task UpdateAlbumAsync()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var album = _context.Albums.Find(_album.Id);

                    album.Title = AlbumTitle.Text;

                    if (_coverPath != null)
                    {
                        int coverId = _context.AlbumCovers.Max(ac => ac.Id) + 1;
                        var albumCover = PictureService.SaveAlbumCover(_coverPath, coverId);

                        _context.Add(albumCover);
                        await _context.SaveChangesAsync();

                        album.CoverId = albumCover.Id;
                    }
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    Debug.WriteLine("Успешное обновление");

                    Close();
                }
                catch (Exception ex)
                {
                    ErrorNoticePanel.Text = "Ошибка изменения альбома.";
                }
            }
        }
    }
}
