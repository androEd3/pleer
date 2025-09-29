using Microsoft.EntityFrameworkCore;
using pleer.Models.Users;
using pleer.Models.Media;
using System.ComponentModel;

namespace pleer.Models.CONTEXT
{
    public class DBContext : DbContext
    {
        public DBContext() : base()
        { }

        //Users
        public DbSet<User> Users { get; set; }
        public DbSet<Artist> Artists { get; set; }

        //Media
        public DbSet<Song> Songs { get; set; }
        public DbSet<AlbumCover> AlbumCovers { get; set; }

        public DbSet<UserPlaylistsLink> UserPlaylistsLinks { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Album> Albums { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=ROONIN-COMPUTAH\\SQLEXPRESS;Database=pleer;Integrated Security=SSPI;Trust Server Certificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Playlist>()
                .Ignore(p => p.Songs);

            modelBuilder.Entity<UserPlaylistsLink>(entity =>
            {
                // Установка составного ключа
                entity.HasKey(upl => new { upl.UserId, upl.PlaylistId });

                // Настройка отношения с User
                entity.HasOne(upl => upl.User)
                      .WithMany(u => u.UserPlaylists)
                      .HasForeignKey(upl => upl.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Настройка отношения с Playlist
                entity.HasOne(upl => upl.Playlist)
                      .WithMany(p => p.UserPlaylists)
                      .HasForeignKey(upl => upl.PlaylistId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
