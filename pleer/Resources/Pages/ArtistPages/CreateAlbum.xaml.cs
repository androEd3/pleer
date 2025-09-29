using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace pleer.Resources.Pages.ArtistPages
{
    /// <summary>
    /// Логика взаимодействия для LoadSongToDB.xaml
    /// </summary>
    public partial class CreateAlbum : Page
    {
        ArtistMainWindow _mainWindow;

        AlbumCover _cover = new();
        Album _album = new();

        Artist _artist;

        public CreateAlbum(ArtistMainWindow main, Artist artist)
        {
            InitializeComponent();

            _mainWindow = main;
            _artist = artist;
        }

        private void SelectAlbumCoverButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выберите обложку альбома",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp|Все файлы|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _cover.FilePath = openFileDialog.FileName;
                    var bitmap = new BitmapImage(new Uri(_cover.FilePath));

                    if (bitmap.PixelWidth != 300 || bitmap.PixelHeight != 300)
                    {
                        MessageBox.Show("Рекомендуется использовать изображение размером 300x300 пикселей. " +
                                        "Текущее изображение не будет масштабировано.");
                    }

                    _album.Title = AlbumTitle.Text;
                    _album.ArtistId = _artist.Id;

                    AlbumCoverDemonstrate.Source = UIServiceMethods.DecodePhoto(_cover.FilePath, 100);
                    AlbumCoverPath.Text = _cover.FilePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
        }

        private void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateMethods.AddSongsToAlbum(_mainWindow, _album, _cover);
        }
    }
}
