using MaterialDesignThemes.Wpf;
using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Users;
using pleer.Resources.Pages.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace pleer.Models.Service
{
    public class UIElementsFactory()
    {
        // SONG CARDS
        public static Border CreateSongCard(
            Song song,
            int id,
            Listener listener,
            Action<object, MouseButtonEventArgs> clickHandler,
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            Grid songGrid;

            if (song.Status)
                songGrid = BlockedSongGrid(song, id);
            else
                songGrid = SongGrid(song, id);

            Grid addButton = new();
            if (!song.Status)
            {
                addButton = CreateAddSongButton(listener, song);
                if (addButton != default)
                {
                    Grid.SetColumn(addButton, 3);
                    songGrid.Children.Add(addButton);
                }
            }

            var border = new Border
            {
                Style = Application.Current.TryFindResource("SimpleFunctionalCard") as Style,
                Margin = new Thickness(5, 0, 5, 5),
                Cursor = Cursors.Hand,
                Child = songGrid,
                Tag = song
            };

            if (!song.Status)
            {
                border.MouseLeftButtonUp += (sender, e) => clickHandler(sender, e);

                if (addButton != null)
                {
                    border.MouseEnter += (s, e) => addButton.Visibility = Visibility.Visible;
                    border.MouseLeave += (s, e) => addButton.Visibility = Visibility.Collapsed;
                }
            }

            return border;
        }

        public static Border CreateSongCard(
            Song song,
            int id,
            Action<object, MouseButtonEventArgs> clickHandler,
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            Grid songGrid;

            if (song.Status)
                songGrid = BlockedSongGrid(song, id);
            else
                songGrid = SongGrid(song, id);

            var border = new Border
            {
                Style = Application.Current.TryFindResource("SimpleFunctionalCard") as Style,
                Margin = new Thickness(5, 0, 5, 5),
                Cursor = Cursors.Hand,
                Child = songGrid,
                Tag = song
            };

            if (!song.Status)
            {
                border.MouseLeftButtonUp += (sender, e) => clickHandler(sender, e);
            }

            return border;
        }

        public static Border CreateSongCard(
            int id,
            Song song,
            Action<object, RoutedEventArgs> clickHandler,
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            var songGrid = SongGrid(song, id);

            var border = new Border
            {
                Style = Application.Current.TryFindResource("SimpleFunctionalCard") as Style,
                Margin = new Thickness(5, 0, 5, 5),
                Cursor = Cursors.Hand,
                Child = songGrid,
                Tag = song
            };

            var banButton = CreateBanButton();
            Grid.SetColumn(banButton, 4);
            songGrid.Children.Add(banButton);

            if (song.Status)
            {
                banButton.Foreground = ColorConvert("#a33");
                banButton.Background = ColorConvert("#eee");
                var infoBlock = new TextBlock
                {
                    Text = "Заблокирована",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10),
                    Style = Application.Current.TryFindResource("SmallErrorPanel") as Style
                };
                Grid.SetColumn(infoBlock, 3);
                songGrid.Children.Add(infoBlock);
            }

            banButton.Tag = song;
            banButton.Click += (sender, e) => clickHandler(sender, e);

            return border;
        }

        public static Border CreateSongCard(
            Song song,
            int id,
            CreateAlbum main,
            Action<object, MouseButtonEventArgs> clickHandler,
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            var songGrid = SongGrid(song, id, main);

            var border = new Border
            {
                Style = Application.Current.TryFindResource("SimpleFunctionalCard") as Style,
                Margin = new Thickness(5, 0, 5, 5),
                Cursor = Cursors.Hand,
                Child = songGrid,
                Tag = song
            };
            border.MouseLeftButtonUp += (sender, e) => clickHandler(sender, e);

            return border;
        }

        public static Grid SongGrid(
            Song song,
            int id,
            CreateAlbum main,
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            using var context = new DBContext();

            var songData = context.Songs
                .Where(s => s.Id == song.Id)
                .Select(s => new
                {
                    Song = s,
                    s.Title,
                    s.TotalDuration,
                })
                .FirstOrDefault();

            var grid = new Grid
            {
                Height = settings.Height,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto},
                    new ColumnDefinition(),
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                }
            };

            var idSong = new TextBlock()
            {
                Text = (id + 1).ToString(),
                Margin = new Thickness(15, 0, 15, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Style = (Style)Application.Current.TryFindResource("SmallInfoPanel"),
            };
            Grid.SetColumn(idSong, 0);
            grid.Children.Add(idSong);

            var infoPanel = CreateInfoPanel(song.Title);
            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            var removeButton = CreateRemoveSongButton();
            Grid.SetColumn(removeButton, 2);
            grid.Children.Add(removeButton);

            removeButton.Click += (s, e) =>
            {
                main.RemoveSongFromAlbum(song);
            };

            var totalDuration = new TextBlock()
            {
                Text = FormattingTotalTime(song),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 15, 0),
                Style = Application.Current.TryFindResource("SmallInfoPanel") as Style,
            };
            Grid.SetColumn(totalDuration, 3);
            grid.Children.Add(totalDuration);

            return grid;
        }

        public static Grid SongGrid(
            Song song,
            int id,
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            using var context = new DBContext();

            var songData = context.Songs
                .Where(s => s.Id == song.Id)
                .Select(s => new
                {
                    Song = s,
                    s.Title,
                    s.Album.Creator,
                    s.Album.Cover,
                    s.TotalPlays,
                })
                .FirstOrDefault();

            string coverPath = songData?.Cover != null ?
                coverPath = songData?.Cover.FilePath :
                coverPath = InitializeData.GetDefaultCoverPath();

            var grid = new Grid
            {
                Height = settings.Height,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto},
                    new ColumnDefinition { Width = new GridLength(settings.Height) },
                    new ColumnDefinition(),
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                }
            };

            var idSong = new TextBlock()
            {
                Text = (id + 1).ToString(),
                Margin = new Thickness(15, 0, 15, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Style = (Style)Application.Current.TryFindResource("SmallInfoPanel"),
            };
            Grid.SetColumn(idSong, 0);
            grid.Children.Add(idSong);

            var imageGrid = CreateCollectionCover(coverPath, settings);
            Grid.SetColumn(imageGrid, 1);
            grid.Children.Add(imageGrid);

            var infoPanel = CreateInfoPanel(songData?.Title, songData?.Creator.Name);
            Grid.SetColumn(infoPanel, 2);
            grid.Children.Add(infoPanel);


            var totalPlays = new TextBlock()
            {
                Text = songData?.TotalPlays.ToString(),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(25, 0, 10, 0),
                Style = Application.Current.TryFindResource("SmallInfoPanel") as Style,
            };
            Grid.SetColumn(totalPlays, 4);
            grid.Children.Add(totalPlays);

            var totalDuration = new TextBlock()
            {
                Text = FormattingTotalTime(song),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 15, 0),
                Style = Application.Current.TryFindResource("SmallInfoPanel") as Style,
            };
            Grid.SetColumn(totalDuration, 5);
            grid.Children.Add(totalDuration);

            return grid;
        }

        public static Grid BlockedSongGrid(
            Song song,
            int id,
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            using var context = new DBContext();

            var songData = context.Songs
                .Where(s => s.Id == song.Id)
                .Select(s => new
                {
                    Song = s,
                    s.Title,
                    s.Album.Creator,
                })
                .FirstOrDefault();

            string coverPath = InitializeData.GetDefaultCoverPath();

            var grid = new Grid
            {
                Height = settings.Height,
                IsEnabled = false,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto},
                    new ColumnDefinition { Width = new GridLength(settings.Height) },
                    new ColumnDefinition(),
                    new ColumnDefinition { Width = GridLength.Auto },
                }
            };

            var idSong = new TextBlock()
            {
                Text = (id + 1).ToString(),
                Margin = new Thickness(15, 0, 15, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Style = (Style)Application.Current.TryFindResource("SmallInfoPanel"),
            };
            Grid.SetColumn(idSong, 0);
            grid.Children.Add(idSong);

            var imageGrid = CreateCollectionCover(coverPath, settings);
            imageGrid.IsEnabled = false;

            Grid.SetColumn(imageGrid, 1);
            grid.Children.Add(imageGrid);

            var infoPanel = CreateInfoPanel(songData?.Title, songData?.Creator.Name);
            infoPanel.IsEnabled = false;

            Grid.SetColumn(infoPanel, 2);
            grid.Children.Add(infoPanel);

            var totalDuration = new TextBlock()
            {
                Text = FormattingTotalTime(song),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 15, 0),
                Style = Application.Current.TryFindResource("SmallInfoPanel") as Style,
            };
            Grid.SetColumn(totalDuration, 3);
            grid.Children.Add(totalDuration);

            return grid;
        }

        // SONG METADATA FILLING
        static Border CreateCollectionCover(string cover, CardSettings settings)
        {
            var imagePath = cover ?? InitializeData.GetDefaultCoverPath();

            if (string.IsNullOrEmpty(cover) || !File.Exists(cover))
                imagePath = InitializeData.GetDefaultCoverPath();
            else
                imagePath = cover;

            var imageSource = DecodePhoto(imagePath, settings.ImageSize * 2);

            return new Border
            {
                Width = settings.ImageSize,
                Height = settings.ImageSize,
                Margin = new Thickness(5),
                CornerRadius = new CornerRadius(5),
                Background = new ImageBrush
                {
                    ImageSource = imageSource,
                    Stretch = Stretch.UniformToFill
                }
            };
        }

        static StackPanel CreateInfoPanel(string title, string artistName)
        {
            var panel = new StackPanel
            {   
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new TextBlock
                    {
                        Name = "SongTitleTextBlock",
                        Text = title,
                        Style = Application.Current.TryFindResource("SmallMainInfoPanel") as Style
                    },
                    new TextBlock
                    {
                        Text = artistName ?? "Unknown Artist",
                        TextWrapping = TextWrapping.Wrap,
                        Style = Application.Current.TryFindResource("SmallInfoPanel") as Style
                    }
                }
            };
            return panel;
        }

        static StackPanel CreateInfoPanel(string title)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new TextBlock
                    {
                        Name = "SongTitleTextBlock",
                        Text = title,
                        Style = Application.Current.TryFindResource("SmallMainInfoPanel") as Style
                    },
                }
            };
            return panel;
        }

        // BUTTONS IN CARD
        public static Grid CreateAddSongButton(
            Listener listener,
            Song song,
            PlacementMode Placement = PlacementMode.Bottom)
        {
            if (listener == default)
                return default;

            var grid = new Grid()
            {
                Visibility = Visibility.Collapsed,
            };

            var icon = new PackIcon
            {
                Width = 25,
                Height = 25,
                Kind = PackIconKind.PlaylistPlus
            };

            var toggleButton = new ToggleButton
            {
                Height = 25,
                Width = 25,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 10, 0),
                Content = icon,
                Style = Application.Current.TryFindResource("AddSongButton") as Style
            };

            var border = new Border
            {
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(0, 5, 0, 5),
                BorderThickness = new Thickness(1),
                BorderBrush = ColorConvert("#333"),
                Style = Application.Current.TryFindResource("NonFunctionalField2c") as Style,
                MaxHeight = 300
            };

            var playlistsPanel = new StackPanel();

            var scrollViewer = new ScrollViewer
            {
                Content = playlistsPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 350
            };

            border.Child = scrollViewer;

            var popup = new Popup
            {
                PlacementTarget = toggleButton,
                Placement = Placement,
                StaysOpen = false,
                AllowsTransparency = true,
                Child = border
            };

            popup.Opened += (s, e) =>
            {
                grid.Visibility = Visibility.Visible;
                RefreshPlaylistsPanel(playlistsPanel, listener, song);
            };
            popup.Closed += (s, e) => grid.Visibility = Visibility.Collapsed;

            var binding = new Binding("IsChecked")
            {
                Source = toggleButton,
                Mode = BindingMode.TwoWay
            };
            popup.SetBinding(Popup.IsOpenProperty, binding);

            grid.Children.Add(toggleButton);
            grid.Children.Add(popup);

            RefreshPlaylistsPanel(playlistsPanel, listener, song);

            return grid;
        }

        static void RefreshPlaylistsPanel(StackPanel playlistsPanel, Listener listener, Song song)
        {
            playlistsPanel.Children.Clear();

            using var context = new DBContext();

            var playlists = context.Playlists
                .Where(p => p.CreatorId == listener.Id)
                .ToList();

            foreach (var playlist in playlists)
            {
                var isSongInPlaylist = playlist.SongsId.Contains(song.Id);

                var playlistIcon = new PackIcon
                {
                    Kind = isSongInPlaylist ? PackIconKind.Check : PackIconKind.Plus,
                    Foreground = isSongInPlaylist ?
                        ColorConvert("#90ee90") :
                        ColorConvert("#eee"),
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(0, 0, 15, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                };

                var playlistText = new TextBlock
                {
                    Text = playlist.Title,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Style = (Style)Application.Current.TryFindResource("SongNameLowerPanel")
                };

                var playlistContent = new Grid()
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition(),
                    }
                };
                playlistContent.Children.Add(playlistIcon);
                Grid.SetColumn(playlistIcon, 0);
                playlistContent.Children.Add(playlistText);
                Grid.SetColumn(playlistText, 1);

                var playlistButton = new Button
                {
                    Content = playlistContent,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left, 
                    Padding = new Thickness(10, 5, 10, 5),
                    Style = (Style)Application.Current.TryFindResource("MenuButton"),
                    Background = ColorConvert("#333"),
                    Tag = playlist.Id
                };

                playlistButton.Click += (s, e) =>
                {
                    var playlistId = (int)(s as Button).Tag;

                    bool isCurrentlyInPlaylist = playlistIcon.Kind == PackIconKind.Check;

                    if (isCurrentlyInPlaylist)
                    {
                        DeleteSongFromPlaylist(playlistId, song.Id);
                        playlistIcon.Kind = PackIconKind.Plus;
                        playlistIcon.Foreground = ColorConvert("#eee");
                    }
                    else
                    {
                        AddSongToPlaylist(playlistId, song.Id);
                        playlistIcon.Kind = PackIconKind.Check;
                        playlistIcon.Foreground = ColorConvert("#90ee90");
                    }
                };

                playlistsPanel.Children.Add(playlistButton);
            }
        }

        static Button CreateRemoveSongButton()
        {
            var icon = new PackIcon
            {
                Width = 25,
                Height = 25,
                Kind = PackIconKind.MinusCircleOutline,
            };

            var buttonStyle = Application.Current.TryFindResource(
                "BaseFunctionButton") as Style;

            var button = new Button
            {
                Height = 30,
                Width = 30,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                Content = icon,
                Style = buttonStyle,
            };

            return button;
        }

        static void AddSongToPlaylist(int playlistId, int songId)
        {
            var context = new DBContext();

            var playlist = context.Playlists.Find(playlistId);
            var song = context.Songs.Find(songId);

            if (playlist != null && song != null)
            {
                playlist.SongsId.Add(song.Id);
                context.SaveChanges();
            }
        }

        static void DeleteSongFromPlaylist(int playlistId, int songId)
        {
            var context = new DBContext();

            var playlist = context.Playlists.Find(playlistId);
            var song = context.Songs.Find(songId);

            if (playlist != null && song != null)
            {
                playlist.SongsId.Remove(song.Id);
                context.SaveChanges();
            }
        }

        // COLLECTION CARDS
        public static Border CreateCollectionCard(
            Playlist playlist,
            Action<object, MouseButtonEventArgs> clickHandler,
            CardSize cardSize = CardSize.Large)
        {
            var settings = GetCardSettings(cardSize);

            var playlistGrid = CollectionGrid(playlist);

            var border = new Border
            {
                Style = (Style)Application.Current.TryFindResource("SimpleFunctionalCard"),
                Margin = new Thickness(5, 0, 5, 5),
                Cursor = Cursors.Hand,
                Child = playlistGrid,
                Tag = playlist
            };
            border.MouseLeftButtonUp += (sender, e) => clickHandler(sender, e);

            return border;
        }

        public static Border CreateCollectionCard(
            Album album,
            Action<object, MouseButtonEventArgs> clickHandler,
            CardSize cardSize = CardSize.Large)
        {
            var settings = GetCardSettings(cardSize);

            var playlistGrid = CollectionGrid(album);

            var border = new Border
            {
                Style = (Style)Application.Current.TryFindResource("SimpleFunctionalCard"),
                Margin = new Thickness(5, 0, 5, 5),
                Cursor = Cursors.Hand,
                Child = playlistGrid,
                Tag = album
            };
            border.MouseLeftButtonUp += (sender, e) => clickHandler(sender, e);

            return border;
        }

        public static Grid CollectionGrid(
            Playlist playlist,
            CardSize cardSize = CardSize.Large)
        {
            var settings = GetCardSettings(cardSize);

            using var context = new DBContext();

            var playlistData = context.Playlists
                .Where(p => p.Id == playlist.Id)
                .Select(p => new
                {
                    Playlist = p,
                    p.Title,
                    p.Creator,
                    p.Cover
                })
                .FirstOrDefault();

            var title = playlistData?.Title;
            var creator = playlistData?.Creator;
            var cover = playlistData?.Cover;

            var playlistGrid = new Grid
            {
                Height = settings.Height,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(settings.Height) },
                    new ColumnDefinition()
                }
            };

            var imageGrid = CreateCollectionCover(cover.FilePath, settings);
            Grid.SetColumn(imageGrid, 0);
            playlistGrid.Children.Add(imageGrid);

            var infoPanel = CreateInfoPanel(title, $"Плейлист •︎ {creator?.Name}");
            Grid.SetColumn(infoPanel, 1);
            playlistGrid.Children.Add(infoPanel);

            return playlistGrid;
        }

        public static Grid CollectionGrid(
            Album album,
            CardSize cardSize = CardSize.Large)
        {
            var settings = GetCardSettings(cardSize);

            using var context = new DBContext();

            var albumData = context.Albums
                .Where(p => p.Id == album.Id)
                .Select(p => new
                {
                    Album = p,
                    p.Title,
                    p.Creator,
                    p.Cover
                })
                .FirstOrDefault();

            var title = albumData?.Title;
            var creator = albumData?.Creator;
            var cover = albumData?.Cover;

            var albumGrid = new Grid
            {
                Height = settings.Height,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(settings.Height) },
                    new ColumnDefinition()
                }
            };

            var imageGrid = CreateCollectionCover(cover.FilePath, settings);
            Grid.SetColumn(imageGrid, 0);
            albumGrid.Children.Add(imageGrid);

            var infoPanel = CreateInfoPanel(title, $"Альбом •︎ {creator?.Name}");
            Grid.SetColumn(infoPanel, 1);
            albumGrid.Children.Add(infoPanel);

            return albumGrid;
        }

        //Artist card
        public static Border CreateArtistCard(
            Artist artist,
            Action<object, MouseButtonEventArgs> clickHandler,
            CardSize cardSize = CardSize.Large)
        {
            var settings = GetCardSettings(cardSize);

            var playlistGrid = ArtistGrid(artist);

            var border = new Border
            {
                Style = (Style)Application.Current.TryFindResource("SimpleFunctionalCard"),
                Margin = new Thickness(5, 0, 5, 5),
                Cursor = Cursors.Hand,
                Child = playlistGrid,
                Tag = artist
            };
            border.MouseLeftButtonUp += (sender, e) => clickHandler(sender, e);

            return border;
        }

        public static Grid ArtistGrid(
            Artist artist,
            CardSize cardSize = CardSize.Large)
        {
            var settings = GetCardSettings(cardSize);

            using var context = new DBContext();

            var artistData = context.Artists
                .Where(a => a.Id == artist.Id)
                .Select(a => new
                {
                    Artist = a,
                    a.Name,
                    a.ProfilePicture
                })
                .FirstOrDefault();

            var albumGrid = new Grid
            {
                Height = settings.Height,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition()
                }
            };

            var imageGrid = CreateArtistProfilePicture(artistData?.ProfilePicture.FilePath, settings);
            Grid.SetColumn(imageGrid, 0);
            albumGrid.Children.Add(imageGrid);

            var infoPanel = CreateInfoPanel(artistData?.Name);
            Grid.SetColumn(infoPanel, 1);
            albumGrid.Children.Add(infoPanel);

            return albumGrid;
        }

        static Ellipse CreateArtistProfilePicture(string cover, CardSettings settings)
        {
            var imagePath = cover ?? InitializeData.GetDefaultProfilePicturePath();

            if (string.IsNullOrEmpty(cover) || !File.Exists(cover))
                imagePath = InitializeData.GetDefaultCoverPath();
            else
                imagePath = cover;

            var imageSource = DecodePhoto(imagePath, settings.ImageSize * 2);

            return new Ellipse
            {
                Width = settings.ImageSize,
                Height = settings.ImageSize,
                Margin = new Thickness(10, 5, 10, 5),
                Fill = new ImageBrush
                {
                    ImageSource = imageSource,
                    Stretch = Stretch.UniformToFill
                }
            };
        }

        // USERS card for Admin
        public static Border CreateUserCard(
            int listenerId,
            Listener listener,
            Action<object, RoutedEventArgs> clickHandler,
            CardSize cardSize = CardSize.Large)
        {
            var settings = GetCardSettings(cardSize);

            using var context = new DBContext();

            var listenerData = context.Listeners
                .Where(l => l.Id == listener.Id)
                .Select(l => new
                {
                    Listener = l,
                    l.Name,
                    l.ProfilePicture
                })
                .FirstOrDefault();

            var listenerGrid = UserGrid(listenerId, listenerData?.Name, listenerData?.ProfilePicture);

            var border = new Border
            {
                Style = (Style)Application.Current.TryFindResource("SimpleFunctionalCard"),
                Margin = new Thickness(5, 0, 5, 5),
                Child = listenerGrid,
            };

            var banButton = CreateBanButton();
            Grid.SetColumn(banButton, 3);
            listenerGrid.Children.Add(banButton);

            if (listener.Status)
            {
                banButton.Foreground = ColorConvert("#a33");
                banButton.Background = ColorConvert("#eee");
                var infoBlock = new TextBlock
                {
                    Text = "Заблокирован",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10),
                    Style = Application.Current.TryFindResource("SmallErrorPanel") as Style
                };
                Grid.SetColumn(infoBlock, 2);
                listenerGrid.Children.Add(infoBlock);
            }

            banButton.Tag = listener;
            banButton.Click += (sender, e) => clickHandler(sender, e);

            return border;
        }

        public static Border CreateUserCard(
            int artistId,
            Artist artist,
            Action<object, RoutedEventArgs> clickHandler,
            CardSize cardSize = CardSize.Large)
        {
            var settings = GetCardSettings(cardSize);

            using var context = new DBContext();

            var artistData = context.Artists
                .Where(a => a.Id == artist.Id)
                .Select(a => new
                {
                    Artist = a,
                    a.Name,
                    a.ProfilePicture
                })
                .FirstOrDefault();

            var artistGrid = UserGrid(artistId, artistData?.Name, artistData?.ProfilePicture);

            var border = new Border
            {
                Style = (Style)Application.Current.TryFindResource("SimpleFunctionalCard"),
                Margin = new Thickness(5, 0, 5, 5),
                Child = artistGrid,
            };

            var banButton = CreateBanButton();
            Grid.SetColumn(banButton, 3);
            artistGrid.Children.Add(banButton);

            if (artist.Status)
            {
                banButton.Foreground = ColorConvert("#a33");
                banButton.Background = ColorConvert("#eee");
                var infoBlock = new TextBlock
                {
                    Text = "Заблокирован",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10),
                    Style = Application.Current.TryFindResource("SmallErrorPanel") as Style
                };
                Grid.SetColumn(infoBlock, 2);
                artistGrid.Children.Add(infoBlock);
            }

            banButton.Tag = artist;
            banButton.Click += (sender, e) => clickHandler(sender, e);

            return border;
        }

        public static Grid UserGrid(
            int id,
            string Name,
            ProfilePicture ProfilePicture,
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            var listenerGrid = new Grid
            {
                Height = settings.Height,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition(),
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                }
            };

            var idSong = new TextBlock()
            {
                Text = (id + 1).ToString(),
                Margin = new Thickness(15, 0, 15, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Style = (Style)Application.Current.TryFindResource("SmallInfoPanel"),
            };
            Grid.SetColumn(idSong, 0);
            listenerGrid.Children.Add(idSong);

            var imageGrid = CreateArtistProfilePicture(ProfilePicture.FilePath, settings);
            Grid.SetColumn(imageGrid, 0);
            listenerGrid.Children.Add(imageGrid);

            var infoPanel = CreateInfoPanel(Name);
            Grid.SetColumn(infoPanel, 1);
            listenerGrid.Children.Add(infoPanel);

            return listenerGrid;
        }

        static Button CreateBanButton()
        {
            var icon = new PackIcon
            {
                Width = 25,
                Height = 25,
                Kind = PackIconKind.BlockHelper,
            };

            var buttonStyle = Application.Current.TryFindResource(
                "DeleteButton") as Style;

            var button = new Button
            {
                Height = 35,
                Width = 35,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 15, 0),
                Content = icon,
                Style = buttonStyle,
            };

            return button;
        }


        //Other методы
        public static BitmapImage ResizeImageTo300x300(string filePath)
        {
            var originalBitmap = new BitmapImage();
            originalBitmap.BeginInit();
            originalBitmap.UriSource = new Uri(filePath);
            originalBitmap.CacheOption = BitmapCacheOption.OnLoad;
            originalBitmap.EndInit();

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(originalBitmap,
                    new Rect(0, 0, 300, 300));
            }

            var resizedBitmap = new RenderTargetBitmap(
                300, 300, 96, 96, PixelFormats.Pbgra32);
            resizedBitmap.Render(drawingVisual);

            var bitmapImage = new BitmapImage();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(resizedBitmap));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        public static BitmapImage DecodePhoto(string resourcePath, int containerWidth, double scaleFactor = 1.75)
        {
            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(resourcePath);
            bitmap.DecodePixelWidth = (int)(containerWidth * scaleFactor);
            bitmap.CreateOptions = BitmapCreateOptions.DelayCreation;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return bitmap;
        }

        public static string FormattingTotalTime(List<Song> collection)
        {
            var summaryDuration = TimeSpan.Zero;

            foreach (var song in collection)
            {
                if (song != default)
                    summaryDuration += song.TotalDuration;
            }

            string formattedDuration;

            if (summaryDuration.TotalHours >= 1)
                formattedDuration = summaryDuration.ToString(@"hh\:mm\:ss");
            else
                formattedDuration = summaryDuration.ToString(@"mm\:ss");

            return formattedDuration;
        }

        public static string FormattingTotalTime(Song song)
        {
            string formattedDuration;

            if (song.TotalDuration.TotalHours >= 1)
                formattedDuration = song.TotalDuration.ToString(@"hh\:mm\:ss");
            else
                formattedDuration = song.TotalDuration.ToString(@"mm\:ss");

            return formattedDuration;
        }

        public static void SetCardTitleColor(Border card, string hexColor)
        {
            if (card?.Child is not Grid songGrid) return;

            var infoPanel = songGrid.Children.OfType<StackPanel>().FirstOrDefault();

            if (infoPanel?.Children.Count > 0 && infoPanel.Children[0] is TextBlock titleTextBlock)
            {
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                titleTextBlock.Foreground = new SolidColorBrush(color);
            }
        }

        public static void NoContent(string message, UIElement parent)
        {
            var infoPanel = new TextBlock()
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 15, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = Application.Current.TryFindResource("SmallInfoPanel") as Style,
            };

            if (parent is StackPanel stackPanel)
                stackPanel.Children.Add(infoPanel);
        }

        public static SolidColorBrush ColorConvert(string hex)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }


        //assist METHODS
        public enum CardSize
        {
            Small,   // обложка для песни
            Large     // обложка для плейлистов/альбомов
        }

        public class CardSettings
        {
            public int Height { get; set; }
            public int ImageSize { get; set; }
        }

        static CardSettings GetCardSettings(CardSize size)
        {
            return size switch
            {
                CardSize.Small => new CardSettings
                {
                    Height = 55,
                    ImageSize = 45,
                },
                CardSize.Large => new CardSettings
                {
                    Height = 80,
                    ImageSize = 70,
                },
                _ => throw new ArgumentOutOfRangeException(nameof(size))
            };
        }
    }
}
