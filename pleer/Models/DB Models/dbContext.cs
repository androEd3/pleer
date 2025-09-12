using Microsoft.EntityFrameworkCore;
using pleer.Models.Users;
using pleer.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pleer.Models.CONTEXT
{
    public class dbContext : DbContext
    {
        public dbContext() : base()
        { }

        //Users
        public DbSet<User> Users { get; set; }
        public DbSet<Artist> Artists { get; set; }

        //Media
        public DbSet<Song> Songs { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<AlbumCover> AlbumCovers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=ROONIN-COMPUTAH\\SQLEXPRESS;Database=pleer;Integrated Security=SSPI;Trust Server Certificate=True");
            }
        }
    }
}
