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
        public static async void SeedData()
        {
            DBContext context = new();

            //Seed covers
            if (!context.AlbumCovers.Any())
            {
                var covers = new List<AlbumCover>()
                {
                    { new() { FilePath = "/Resources/ServiceImages/Favorites.png"} },
                    { new() { FilePath = "/Resources/ServiceImages/NoMediaImage.png"} },
                };
                await context.AddRangeAsync(covers);
                await context.SaveChangesAsync();
            }
        }
    }
}
