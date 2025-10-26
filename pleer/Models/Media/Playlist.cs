using pleer.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace pleer.Models.Media
{
    public class Playlist
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        public int CoverId { get; set; }
        public PlaylistCover Cover { get; set; }

        public int CreatorId { get; set; }
        public Listener Creator { get; set; }

        [Required]
        public DateOnly CreationDate { get; set; }

        public List<int> SongsId { get; set; } = new List<int>();
        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();

        //навигация
        public virtual ICollection<ListenerPlaylistsLink> ListenerPlaylists { get; set; } = new List<ListenerPlaylistsLink>();
    }
}
