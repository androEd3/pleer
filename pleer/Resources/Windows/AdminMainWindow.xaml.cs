using pleer.Models.DatabaseContext;
using pleer.Models.Service;
using pleer.Models.Users;
using pleer.Resources.Pages.AdminPages;
using pleer.Resources.Pages.GeneralPages;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для AdminMainWindow.xaml
    /// </summary>
    public partial class AdminMainWindow : Window
    {
        DBContext _context = new();

        Admin _admin;

        public AdminMainWindow()
        {
            InitializeComponent();

            LoadNonUserWindow();
        }

        public AdminMainWindow(Admin admin)
        {
            InitializeComponent();

            _admin = admin;
        }

        void LoadNonUserWindow()
        {
            InitializeData.CreateAdmin(_context);
            FullWindow.Navigate(new LoginPage(this));
        }

        // MAIN operations
        private void UsersListButton_Click(object sender, RoutedEventArgs e)
        {
            OperationField.Navigate(new UsersBanListPage());
        }

        private void SongsListButton_Click(object sender, RoutedEventArgs e)
        {
            OperationField.Navigate(new SongsBanListPage());
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            OperationField.Navigate(new ReportPage());
        }

        // login as ktoto tam
        private void LoginAsListenerButton_Click(object sender, RoutedEventArgs e)
        {
            new ListenerMainWindow().Show(); Close();
        }

        private void LoginAsArtistButton_Click(object sender, RoutedEventArgs e)
        {
            new ArtistMainWindow().Show(); Close();
        }

        // PAGE navigation
        private void BackPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (OperationField.CanGoBack)
                OperationField.GoBack();
        }

        private void ForwardPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (OperationField.CanGoForward)
                OperationField.GoForward();
        }

        private void UpdateButtonState()
        {
            BackButton.IsEnabled = OperationField.CanGoBack;
            ForwardButton.IsEnabled = OperationField.CanGoForward;
        }

        private void OperationField_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            UpdateButtonState();
        }
    }
}
