using Microsoft.EntityFrameworkCore;
using pleer.Models.CONTEXT;
using pleer.Models.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace pleer
{
    /// <summary>
    /// Логика взаимодействия для UploadSongWindow.xaml
    /// </summary>
    public partial class UploadSongWindow : Window
    {
        dbContext _context = new dbContext();

        string _albumCoverPath;
        string _songPath;

        public UploadSongWindow()
        {
            InitializeComponent();
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
                    _albumCoverPath = openFileDialog.FileName;
                    var bitmap = new BitmapImage(new Uri(_albumCoverPath));

                    if (bitmap.PixelWidth != 300 || bitmap.PixelHeight != 300)
                    {
                        MessageBox.Show("Рекомендуется использовать изображение размером 300x300 пикселей. " +
                                        "Текущее изображение не будет масштабировано.");
                    }

                    AlbumCoverPath.Text = _albumCoverPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
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
                    _songPath = openFileDialog.FileName;

                    SongPath.Text = _songPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки файла: {ex.Message}");
                }
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private void UploadSongButton_Click(object sender, RoutedEventArgs e)
        {
            var newAlbumCover = new AlbumCover()
            {
                AlbumCoverPath = _albumCoverPath
            };

            var newSong = new Song()
            {
                Title = SongName.Text,
                Artist = CreatorName.Text,
                AlbumCoverId = newAlbumCover.Id,
                SongPath = _songPath
            };

            try
            {
                _context.AlbumCovers.Add(newAlbumCover);
                _context.Songs.Add(newSong);

                _context.SaveChanges();

                MessageBox.Show("Песня и обложка успешно добавлена на площадку pleer");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузка песни: " + ex.Message);
            }
        }
    }
}
