using pleer.Models.CONTEXT;
using pleer.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pleer.Models.Media
{
    public class Playlist
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public int CreatorId { get; set; }
        public User Creator { get; set; }

        [Required]
        public DateOnly CreationDate { get; set; }

        [Required]
        public int AlbumCoverId { get; set; }

        public List<int> SongsId { get; set; } = new List<int>();
        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();

        //навигация
        public virtual ICollection<UserPlaylistsLink> UserPlaylists { get; set; } = new List<UserPlaylistsLink>();
    }
}
