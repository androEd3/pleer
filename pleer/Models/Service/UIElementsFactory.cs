using MaterialDesignThemes.Wpf;
using pleer.Models.DatabaseContext;
using pleer.Models.Media;
using pleer.Models.Users;
using pleer.Resources.Pages.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace pleer.Models.Service
{
    public class UIElementsFactory()
    {
        // SONG CARDS
        public static Border CreateSongCard(
            Song song,
            Listener listener,
            Action<object, MouseButtonEventArgs> clickHandler,
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            var songGrid = SongGrid(song);

            if (listener != null)
            {
                var addButton = CreateAddSongButton(listener, song);
                if (addButton != null)
                {
                    Grid.SetColumn(addButton, 2);
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
            border.MouseLeftButtonUp += (sender, e) => clickHandler(sender, e);

            return border;
        }

        public static Border CreateSongCard(
            Song song,
            Action<object, MouseButtonEventArgs> clickHandler,
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            var songGrid = SongGrid(song);

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

        public static Border CreateSongCard(
            Song song,
            CreateAlbum main,
            Action<object, MouseButtonEventArgs> clickHandler,
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            var songGrid = SongGrid(song);

            var removeButton = CreateRemoveSongButton();
            if (removeButton != null)
            {
                Grid.SetColumn(removeButton, 2);
                songGrid.Children.Add(removeButton);

                removeButton.Click += (s, e) =>
                {
                    main.RemoveSongFromAlbum(song);
                };
            }

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
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            using var context = new DBContext();

            var songData = context.Songs
                .Where(s => s.Id == song.Id)
                .Select(s => new
                {
                    Song = s,
                    s.Album.Creator,
                    s.Album.Cover
                })
                .FirstOrDefault();

            var artist = songData?.Creator;
            var cover = songData?.Cover;

            string coverPath = cover != null ?
                cover.FilePath
                : InitializeData.GetDefaultCoverPath();

            var grid = new Grid
            {
                Height = settings.Height,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(settings.Height) },
                    new ColumnDefinition(),
                    new ColumnDefinition { Width = new GridLength(50) }
                }
            };

            var imageGrid = CreateCollectionCover(coverPath, settings);
            Grid.SetColumn(imageGrid, 0);
            grid.Children.Add(imageGrid);

            var infoPanel = CreateInfoPanel(song.Title, artist.Name);
            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            return grid;
        }

        // SONG METADATA FILLING
        static Grid CreateCollectionCover(string cover, CardSettings settings)
        {
            var imagePath = cover ?? InitializeData.GetDefaultCoverPath();
            var imageSource = DecodePhoto(imagePath, settings.ImageSize * 2);

            var albumCover = new Image { Source = imageSource };

            var clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, settings.ImageSize, settings.ImageSize),
                RadiusX = 5,
                RadiusY = 5
            };

            return new Grid
            {
                Width = settings.ImageSize,
                Height = settings.ImageSize,
                Margin = new Thickness(5),
                Clip = clip,
                Children = { albumCover }
            };
        }

        static StackPanel CreateInfoPanel(string title, string artistName)
        {
            var smallMainInfoStyle = Application.Current.TryFindResource("SmallMainInfoPanel") as Style;
            var smallInfoStyle = Application.Current.TryFindResource("SmallInfoPanel") as Style;

            return new StackPanel
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new TextBlock
                    {
                        Text = title,
                        Style = smallMainInfoStyle
                    },
                    new TextBlock
                    {
                       Text = artistName ?? "Unknown Artist",
                       TextWrapping = TextWrapping.Wrap,
                       Style = smallInfoStyle
                    }
                }
            };
        }

        // BUTTONS IN CARD
        static Button CreateAddSongButton(
            Listener listener,
            Song song)
        {
            var context = new DBContext();

            if (listener == null) 
                return null;

            var playlist = context.Playlists
                .FirstOrDefault(p => p.CreatorId == listener.Id);
            if (playlist == null) 
                return null;

            var isSongInPlaylist = playlist.SongsId.Contains(song.Id);

            var icon = new PackIcon
            {
                Width = 25,
                Height = 25,
                Kind = isSongInPlaylist ?
                    PackIconKind.Minus :
                    PackIconKind.Plus
            };

            var buttonStyle = (Style)Application.Current.TryFindResource(
                isSongInPlaylist ? "RemoveSongButton" : "AddSongButton");

            var button = new Button
            {
                Height = 25,
                Width = 25,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                Content = icon,
                Style = buttonStyle,
                Visibility = isSongInPlaylist ? Visibility.Visible : Visibility.Collapsed
            };

            button.Click += (s, e) =>
            {
                if (!playlist.SongsId.Contains(song.Id))
                {
                    AddSongToPlaylist(playlist.Id, song.Id);
                    icon.Kind = PackIconKind.Minus;
                    button.Style = (Style)Application.Current.TryFindResource("RemoveSongButton");
                }
                else
                {
                    DeleteSongFromPlaylist(playlist.Id, song.Id);
                    icon.Kind = PackIconKind.Plus;
                    button.Style = (Style)Application.Current.TryFindResource("AddSongButton");
                }
            };

            return button;
        }

        static Button CreateRemoveSongButton()
        {
            var icon = new PackIcon
            {
                Width = 25,
                Height = 25,
                Kind = PackIconKind.Minus,
            };

            var buttonStyle = Application.Current.TryFindResource(
                "RemoveSongButton") as Style;

            var button = new Button
            {
                Height = 25,
                Width = 25,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                Content = icon,
                Style = buttonStyle,
                Visibility =  Visibility.Visible,
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

        public static UIElement CreateCollectionCard(
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
