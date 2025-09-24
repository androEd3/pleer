using pleer.Models.CONTEXT;
using pleer.Models.Media;
using pleer.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace pleer.Models.DB_Models
{
    // Сервисный класс для работы с плейлистами
    public class DBServiceMethods
    {
        public static void AddPlaylistWithLink(User user)
        {
            try
            {
                dbContext context = new dbContext();

                var playlistCount = context.Playlists.Count(p => p.CreatorId == user.Id);

                var playlist = new Playlist()
                {
                    CreationDate = DateOnly.FromDateTime(DateTime.Now),
                    Title = $"Плейлист {playlistCount}",
                    AlbumCoverId = 1, // nomedia
                    CreatorId = user.Id
                };

                context.Playlists.Add(playlist);
                context.SaveChanges();

                var link = new UserPlaylistsLink()
                {
                    UserId = user.Id,
                    PlaylistId = playlist.Id
                };

                context.UserPlaylistsLinks.Add(link);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании плейлиста", ex.Message);
            }
        }
    }
}
