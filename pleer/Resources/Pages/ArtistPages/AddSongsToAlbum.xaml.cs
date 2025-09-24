using pleer.Models.CONTEXT;
using pleer.Models.Media;
using pleer.Models.ModelsUI;
using pleer.Resources.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pleer.Resources.Pages.ArtistPages
{
    /// <summary>
    /// Логика взаимодействия для AddSongsToAlbum.xaml
    /// </summary>
    public partial class AddSongsToAlbum : Page
    {
        dbContext _context = new dbContext();

        ArtistMainWindow _mainWindow;

        Album _album;
        AlbumCover _cover;

        List<Song> _songRange = new List<Song>();

        public AddSongsToAlbum(ArtistMainWindow main, Album album, AlbumCover cover)
        {
            InitializeComponent();

            _mainWindow = main;

            _album = album;
            _cover = cover;
        }

        private void SelectSongFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
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
                        Title = System.IO.Path.GetFileNameWithoutExtension(songPath),
                        FilePath = songPath,
                    };

                    _songRange.Add(song);

                    UIServiceMethods.CreateSongUri(this, songPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки файла: {ex.Message}");
                }
            }
        }

        private async void UploadAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_songRange.Any())
            {
                MessageBox.Show("Добавьте в альбом хотя бы одну песню");
                return;
            }


            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                _context.AlbumCovers.Add(_cover);
                await _context.SaveChangesAsync();

                _album.AlbumCoverId = _cover.Id;
                _album.ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow);

                _context.Albums.Add(_album);
                await _context.SaveChangesAsync();

                foreach (var song in _songRange)
                {
                    song.AlbumId = _album.Id;
                    _context.Songs.Add(song);
                }
                await _context.SaveChangesAsync();

                _album.SongsId = _songRange.Select(s => s.Id).ToList();
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                MessageBox.Show("Альбом успешно добавлен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузка песни: " + ex.Message);
            }
        }

        private void ReturnToAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateMethods.CreateAlbum(_mainWindow, _album.Artist);
        }
    }
}
