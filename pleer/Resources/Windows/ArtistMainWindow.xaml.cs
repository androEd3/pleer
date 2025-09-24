using pleer.Models.CONTEXT;
using pleer.Models.ModelsUI;
using pleer.Models.Users;
using pleer.Resources.Pages.ArtistPages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace pleer.Resources.Windows
{
    /// <summary>
    /// Логика взаимодействия для ArtistMainWindow.xaml
    /// </summary>
    public partial class ArtistMainWindow : Window
    {
        dbContext _context = new dbContext();

        Artist _artist;

        public ArtistMainWindow()
        {
            InitializeComponent();
        }

        private void LoginAsListinerButton_Click(object sender, RoutedEventArgs e)
        {
            var userMainWindow = new UserMainWindow();
            userMainWindow.Show(); this.Close();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLoginBrowser();
        }
        void OpenLoginBrowser()
        {
            _context = new dbContext();

            try
            {
                _context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            var authUrl = "https://localhost:7021/Home/Index";
            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });
        }

        private void LoadAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            _artist = _context.Artists.Find(1);

            NavigateMethods.CreateAlbum(this, _artist);
        }
    }
}
