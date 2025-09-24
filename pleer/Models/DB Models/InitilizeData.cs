using pleer.Models.DB_Models;
using pleer.Models.Media;
using pleer.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pleer.Models.CONTEXT
{
    public static class InitilizeData
    {
        public static void SeedData()
        {
            dbContext context = new dbContext();

            //Seed songs


            //Seed albums


            //Seed artist
            if (!context.Artists.Any())
            {
                var artist = new Artist()
                {
                    Name = "темны принц",
                    Email = "maloy@a.a",
                    PasswordHash = "zxc228",
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                };
                context.Artists.Add(artist);
                context.SaveChanges();
            }

            //Seed users
            if (!context.Users.Any())
            {
                var user = new User()
                {
                    Name = "хейтер",
                    Email = "bolshoy@a.a",
                    PasswordHash = "zxc1337",
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                };
                context.Users.Add(user);
                context.SaveChanges();

                CreatePlaylist(user);
            }

            //Seed covers
            if (!context.AlbumCovers.Any())
            {
                var cover = new AlbumCover()
                {
                    FilePath = "D:\\pleerMusicAlbumCovers\\inRaingows.png"
                };
                context.AlbumCovers.Add(cover);
                context.SaveChanges();
            }
        }

        static void CreatePlaylist(User user)
        {
            DBServiceMethods.AddPlaylistWithLink(user);
        }
    }
}
