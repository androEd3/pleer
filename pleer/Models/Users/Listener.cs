using pleer.Models.Media;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pleer.Models.Users
{
    public class Listener
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public required string Name { get; set; }

        [MaxLength(255)]
        public required string Email { get; set; }

        public int ProfilePictureId { get; set; }

        [MaxLength(64)]
        public required string PasswordHash { get; set; }

        [Column(TypeName = "date")]
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        //навигация
        public virtual ICollection<ListenerPlaylistsLink> ListenerPlaylists { get; set; } = new List<ListenerPlaylistsLink>();
    }
}
