using pleer.Models.Users;

namespace pleer.Models.Media
{
    public class UserPlaylistsLink
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int PlaylistId { get; set; }
        public virtual Playlist Playlist { get; set; }
    }
}
