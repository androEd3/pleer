using pleer.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace pleer.Models.Media
{
    public class Album
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public int ArtistId { get; set; }
        public Artist Artist { get; set; }

        public DateOnly ReleaseDate { get; set; }

        [Required]
        public int AlbumCoverId { get; set; }

        public List<int> SongsId { get; set; } = [];
        public virtual ICollection<Song> Songs { get; set; } = [];
    }
}
