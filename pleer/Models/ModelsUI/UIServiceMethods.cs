using Microsoft.EntityFrameworkCore;
using pleer.Models.CONTEXT;
using pleer.Models.Media;
using pleer.Models.Users;
using pleer.Resources.Pages;
using pleer.Resources.Pages.ArtistPages;
using pleer.Resources.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace pleer.Models.ModelsUI
{
    public class UIServiceMethods()
    {
        //songs
        public static UIElement CreateSongCard(UserMainWindow main, User user, Song song, 
            CardSize cardSize = CardSize.Small)
        {
            DBContext context = new();

            var album = context.Albums.Find(song.AlbumId);
            var artist = context.Artists.Find(album.ArtistId);
            var cover = context.AlbumCovers.Find(album.AlbumCoverId);

            var settings = GetCardSettings(cardSize);

            Grid grid = new()
            {
                Height = settings.Height,
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(settings.ImageContainerWidth) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

            var albumCover = new Image
            {
                Source = DecodePhoto(cover.FilePath, 90),
            };

            var coverBorder = new Border
            {
                Width = settings.ImageSize,
                Height = settings.ImageSize,
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(15, 5, 5, 5),
                Style = Application.Current.TryFindResource("AlbumCover") as Style,
                Child = albumCover,
            };

            Grid.SetColumn(coverBorder, 0);
            grid.Children.Add(coverBorder);

            //metadata
            var infoPanel = new StackPanel
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
            };

            var title = new TextBlock
            {
                Text = song.Title,
                Style = Application.Current.TryFindResource("SmallMainInfoPanel") as Style
            };

            var artistName = new TextBlock
            {
                Text = artist.Name,
                Style = Application.Current.TryFindResource("SmallInfoPanel") as Style
            };

            infoPanel.Children.Add(title);
            infoPanel.Children.Add(artistName);

            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            var icon = new MaterialDesignThemes.Wpf.PackIcon();
            icon.Width = icon.Height = 25;

            var addButton = new Button();
            bool isSongInPlaylist = false;

            if (user != null)
            {
                addButton = new Button
                {
                    Height = 25,
                    Width = 25,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0),
                    Content = icon,
                };

                var playlist = context.Playlists.First(p => p.CreatorId == user.Id);

                isSongInPlaylist = playlist.SongsId.Contains(song.Id);

                if (isSongInPlaylist)
                {
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Minus; // Иконка минуса для удаления
                    addButton.Style = Application.Current.TryFindResource("RemoveSongButton") as Style;
                    addButton.Visibility = Visibility.Visible;
                }
                else
                {
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Plus; // Иконка плюса для добавления
                    addButton.Style = Application.Current.TryFindResource("AddSongButton") as Style;
                    addButton.Visibility = Visibility.Collapsed;
                }

                addButton.Click += (s, e) =>
                {
                    if (!playlist.SongsId.Contains(song.Id))
                    {
                        AddSongToPlaylist(playlist, song, context);
                        icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Minus;
                        addButton.Style = Application.Current.TryFindResource("RemoveSongButton") as Style;
                    }
                    else
                    {
                        DeleteSongFromPlaylist(playlist, song, context);
                        icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Plus;
                        addButton.Style = Application.Current.TryFindResource("AddSongButton") as Style;
                    }
                };

                Grid.SetColumn(addButton, 2);
                grid.Children.Add(addButton);
            }

            Border border = new()
            {
                Style = Application.Current.TryFindResource("SimpleFunctionalCard") as Style,

                Margin = new Thickness(5, 0, 5, 5),
                Cursor = Cursors.Hand,

                Child = grid,
                Tag = song,
            };

            if (!isSongInPlaylist)
            {
                border.MouseEnter += (s, e) =>
                {
                    if (icon.Kind == MaterialDesignThemes.Wpf.PackIconKind.Plus)
                    {
                        addButton.Visibility = Visibility.Visible;
                    }
                };

                border.MouseLeave += (s, e) =>
                {
                    if (icon.Kind == MaterialDesignThemes.Wpf.PackIconKind.Plus)
                    {
                        addButton.Visibility = Visibility.Collapsed;
                    }
                };
            }

            border.MouseLeftButtonDown += main.SongCard_Click;

            return border;
        }

        static void AddSongToPlaylist(Playlist playlist, Song song, DBContext context)
        {
            playlist.SongsId.Add(song.Id);
            context.SaveChanges();
        }

        static void DeleteSongFromPlaylist(Playlist playlist, Song song, DBContext context)
        {
            playlist.SongsId.Remove(song.Id);
            context.SaveChanges();
        }

        public static UIElement CreatePlaylistCard(SimplePlaylistsList main, Playlist playlist,
            CardSize cardSize = CardSize.Large)
        {
            DBContext context = new();

            var creator = context.Users.Find(playlist.CreatorId);
            var cover = context.AlbumCovers.Find(playlist.AlbumCoverId);

            var settings = GetCardSettings(cardSize);

            Grid grid = new()
            {
                Height = settings.Height,
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(settings.ImageContainerWidth) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var albumCover = new Image
            {
                Source = DecodePhoto(cover.FilePath, 160),
            };

            var coverBorder = new Border
            {
                Width = settings.ImageSize,
                Height = settings.ImageSize,
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(15, 5, 5, 5),
                Child = albumCover,
            };

            Grid.SetColumn(coverBorder, 0);
            grid.Children.Add(coverBorder);

            //metadata
            var infoPanel = new StackPanel
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
            };

            var title = new TextBlock
            {
                Text = playlist.Title,
                Style = Application.Current.TryFindResource("SmallMainInfoPanel") as Style
            };


            var artistName = new TextBlock
            {
                Text = creator.Name,
                Style = Application.Current.TryFindResource("SmallInfoPanel") as Style
            };

            infoPanel.Children.Add(title);
            infoPanel.Children.Add(artistName);

            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            Border border = new()
            {
                Style = Application.Current.TryFindResource("SimpleFunctionalCard") as Style,

                Margin = new Thickness(5),
                Cursor = Cursors.Hand,

                Child = grid,
                Tag = playlist,
            };

            border.MouseLeftButtonDown += main.PlaylistCard_Click;

            return border;
        }

        public static void CreateSongUri(AddSongsToAlbum addSongs, string path)
        {
            TextBlock songPathTextBlock = new()
            {
                Name = "SongPath",
                Text = path,
                Style = Application.Current.TryFindResource("SmallInfoPanel") as Style,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap
            };

            ToolTip toolTip = new();
            TextBlock toolTipTextBlock = new()
            {
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 400
            };

            Binding binding = new("Text")
            {
                Source = songPathTextBlock
            };
            toolTipTextBlock.SetBinding(TextBlock.TextProperty, binding);

            toolTip.Content = toolTipTextBlock;
            songPathTextBlock.ToolTip = toolTip;

            addSongs.songList.Children.Add(songPathTextBlock);
        }

        public static BitmapImage DecodePhoto(string resourcePath, int decodePixelWidth)
        {
            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(resourcePath, UriKind.Relative);
            bitmap.DecodePixelWidth = decodePixelWidth;
            bitmap.EndInit();

            return bitmap;
        }

        //assist METHODS
        public enum CardSize
        {
            Small,   // Стандартный размер (песни)
            Large     // Для плейлистов/альбомов
        }

        public class CardSettings
        {
            public int Height { get; set; }
            public int ImageSize { get; set; }
            public int ImageContainerWidth { get; set; }
        }

        private static CardSettings GetCardSettings(CardSize size)
        {
            return size switch
            {
                CardSize.Small => new CardSettings
                {
                    Height = 55,
                    ImageSize = 45,
                    ImageContainerWidth = 60,
                },
                CardSize.Large => new CardSettings
                {
                    Height = 80,
                    ImageSize = 70,
                    ImageContainerWidth = 90,
                },
                _ => throw new ArgumentOutOfRangeException(nameof(size))
            };
        }
    }
}
