using Microsoft.EntityFrameworkCore;
using pleer.Models.Users;
using pleer.Models.Media;

namespace pleer.Models.DatabaseContext
{
    public class DBContext : DbContext
    {
        public DBContext() : base()
        { }

        //Users
        public DbSet<Listener> Listeners { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<ProfilePicture> ProfilePictures { get; set; }

        //Media
        public DbSet<Song> Songs { get; set; }
        public DbSet<AlbumCover> AlbumCovers { get; set; }
        public DbSet<PlaylistCover> PlaylistCovers { get; set; }

        public DbSet<ListenerPlaylistsLink> ListenerPlaylistsLinks { get; set; }
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

            //modelBuilder.Entity<AlbumCover>()
            //    .Ignore(a => a.Album);

            //modelBuilder.Entity<PlaylistCover>()
            //    .Ignore(p => p.Playlist);

            //modelBuilder.Entity<ProfilePicture>()
            //    .Ignore(p => p.User);

            modelBuilder.Entity<ListenerPlaylistsLink>(entity =>
            {
                // Установка составного ключа
                entity.HasKey(upl => new { upl.ListenerId, upl.PlaylistId });

                // Настройка отношения с User
                entity.HasOne(upl => upl.Listener)
                      .WithMany(u => u.ListenerPlaylists)
                      .HasForeignKey(upl => upl.ListenerId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Настройка отношения с Playlist
                entity.HasOne(upl => upl.Playlist)
                      .WithMany(p => p.ListenerPlaylists)
                      .HasForeignKey(upl => upl.PlaylistId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
