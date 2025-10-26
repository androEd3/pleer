using Microsoft.EntityFrameworkCore;
using pleer.Models.CONTEXT;
using pleer.Models.DB_Models;
using pleer.Models.Media;
using pleer.Models.Users;
using pleer.Resources.Pages.AlbumsAndPlaylists;
using pleer.Resources.Windows;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace pleer.Models.ModelsUI
{
    public class UIElementsFactory()
    {
        //songs
        public static UIElement CreateSongCardListener(
            ListenerMainWindow main,
            Listener listener,
            Song song,
            CardSize cardSize = CardSize.Small)
        {
            var settings = GetCardSettings(cardSize);

            var context = new DBContext();

            var songData = context.Songs
                .Where(s => s.Id == song.Id)
                .Select(s => new
                {
                    Song = s,
                    s.Album,
                    s.Album.Artist,
                    s.Album.Cover
                })
                .FirstOrDefault();

            var album = songData?.Album;
            var artist = songData?.Artist;
            var cover = songData?.Cover;

            var grid = new Grid
            {
                Height = settings.Height,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(settings.Height) },
                    new ColumnDefinition(),
                    new ColumnDefinition { Width = new GridLength(40) }
                }
            };

            var imageGrid = CreateAlbumCover(cover.FilePath, settings);
            Grid.SetColumn(imageGrid, 0);
            grid.Children.Add(imageGrid);

            var infoPanel = CreateInfoPanel(song.Title, artist.Name);
            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            var addButton = CreateAddButton(listener, song, context);
            if (addButton != null)
            {
                Grid.SetColumn(addButton, 2);
                grid.Children.Add(addButton);
            }

            var border = CreateBorderForSongListener(grid, song, main, addButton);
            return border;
        }

        public static UIElement CreateSongCardArtist(
            OpenAlbum main,
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
                    s.Album,
                    s.Album.Artist,
                    s.Album.Cover
                })
                .FirstOrDefault();

            var album = songData?.Album;
            var artist = songData?.Artist;
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
                }
            };

            var imageGrid = CreateAlbumCover(coverPath, settings);
            Grid.SetColumn(imageGrid, 0);
            grid.Children.Add(imageGrid);

            var infoPanel = CreateInfoPanel(song.Title, artist.Name);
            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            var border = CreateBorderForSongArtist(grid, song);

            return border;
        }

        public static UIElement CreateSongCardArtist(
            CreateAlbum main,
            Artist artist,
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
                    s.Album,
                    s.Album.Cover
                })
                .FirstOrDefault();

            var coverPath = InitializeData.GetDefaultCoverPath();

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

            var imageGrid = CreateAlbumCover(coverPath, settings);
            Grid.SetColumn(imageGrid, 0);
            grid.Children.Add(imageGrid);

            var infoPanel = CreateInfoPanel(song.Title, artist.Name);
            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            var addButton = CreateRemoveButton(song, context);
            if (addButton != null)
            {
                Grid.SetColumn(addButton, 2);
                grid.Children.Add(addButton);

                addButton.Click += (s, e) =>
                {
                    main.RemoveSongFromAlbum(song);
                };
            }

            var border = CreateBorderForSongArtist(grid, song);
            return border;
        }

        static Grid CreateAlbumCover(string cover, CardSettings settings)
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
                       Style = smallInfoStyle
                    }
                }
            };
        }

        static Button CreateAddButton(
            Listener listener,
            Song song,
            DBContext context)
        {
            if (listener == null) return null;

            var playlist = context.Playlists.FirstOrDefault(p => p.CreatorId == listener.Id);
            if (playlist == null) return null;

            var isSongInPlaylist = playlist.SongsId.Contains(song.Id);

            var icon = new MaterialDesignThemes.Wpf.PackIcon
            {
                Width = 25,
                Height = 25,
                Kind = isSongInPlaylist ?
                    MaterialDesignThemes.Wpf.PackIconKind.Minus :
                    MaterialDesignThemes.Wpf.PackIconKind.Plus
            };

            var buttonStyle = Application.Current.TryFindResource(
                isSongInPlaylist ? "RemoveSongButton" : "AddSongButton") as Style;

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
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Minus;
                    button.Style = Application.Current.TryFindResource("RemoveSongButton") as Style;
                    button.Visibility = Visibility.Visible;
                }
                else
                {
                    DeleteSongFromPlaylist(playlist.Id, song.Id);
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Plus;
                    button.Style = Application.Current.TryFindResource("AddSongButton") as Style;
                    button.Visibility = Visibility.Collapsed;
                }
            };

            return button;
        }

        static Button CreateRemoveButton(
            Song song,
            DBContext context)
        {
            var icon = new MaterialDesignThemes.Wpf.PackIcon
            {
                Width = 25,
                Height = 25,
                Kind = MaterialDesignThemes.Wpf.PackIconKind.Minus,
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

        static Border CreateBorderForSongArtist(
            Grid grid,
            Song song)
        {
            var border = new Border
            {
                Style = Application.Current.TryFindResource("SimpleFunctionalCard") as Style,
                Margin = new Thickness(5, 0, 5, 5),
                Cursor = Cursors.Hand,
                Child = grid,
                Tag = song
            };

            return border;
        }

        static Border CreateBorderForSongListener(
            Grid grid,
            Song song,
            ListenerMainWindow main,
            Button addButton)
        {
            var border = new Border
            {
                Style = Application.Current.TryFindResource("SimpleFunctionalCard") as Style,
                Margin = new Thickness(5, 0, 5, 5),
                Cursor = Cursors.Hand,
                Child = grid,
                Tag = song
            };

            border.MouseLeftButtonDown += main.SongCard_Click;

            if (addButton != null)
            {
                var icon = addButton.Content as MaterialDesignThemes.Wpf.PackIcon;
                if (icon?.Kind == MaterialDesignThemes.Wpf.PackIconKind.Plus)
                {
                    border.MouseEnter += (s, e) => addButton.Visibility = Visibility.Visible;
                    border.MouseLeave += (s, e) => addButton.Visibility = Visibility.Collapsed;
                }
            }

            return border;
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

        public static UIElement CreatePlaylistCard(
            AlbumsList main,
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

            var grid = new Grid
            {
                Height = settings.Height,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(settings.Height) },
                    new ColumnDefinition()
                }
            };

            var imageGrid = CreateAlbumCover(cover.FilePath, settings);
            Grid.SetColumn(imageGrid, 0);
            grid.Children.Add(imageGrid);

            var infoPanel = CreateInfoPanel(title, creator?.Name);
            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);
            
            var border = CreateBorderForAlbum(grid, playlist, main.PlaylistCard_Click);
            return border;
        }

        public static UIElement CreateAlbumCard(
            AlbumsList main,
            Album album,
            CardSize cardSize = CardSize.Large)
        {
            var settings = GetCardSettings(cardSize);

            using var context = new DBContext();

            var albumData = context.Albums
                .Where(a => a.Id == album.Id)
                .Select(a => new
                {
                    Album = a,
                    a.Cover
                })
                .FirstOrDefault();

            var cover = albumData?.Cover.FilePath;

            var grid = new Grid
            {
                Height = settings.Height,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(settings.Height) },
                    new ColumnDefinition()
                }
            };

            var imageGrid = CreateAlbumCover(cover, settings);
            Grid.SetColumn(imageGrid, 0);
            grid.Children.Add(imageGrid);

            var infoPanel = CreateInfoPanel(album.Title, string.Empty);
            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            var border = CreateBorderForAlbum(grid, album, main.AlbumCard_Click);
            return border;
        }

        static Border CreateBorderForAlbum(Grid grid, object tag, MouseButtonEventHandler clickHandler)
        {
            var border = new Border
            {
                Style = Application.Current.TryFindResource("SimpleFunctionalCard") as Style,
                Margin = new Thickness(5, 0, 5, 5),
                Cursor = Cursors.Hand,
                Child = grid,
                Tag = tag
            };

            border.MouseLeftButtonDown += clickHandler;
            return border;
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
