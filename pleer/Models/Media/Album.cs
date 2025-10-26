using pleer.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace pleer.Models.Media
{
    public class Album
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        public int CoverId { get; set; } = 1;
        public AlbumCover Cover { get; set; }

        public int ArtistId { get; set; }
        public Artist Artist { get; set; }

        [Required]
        public DateOnly ReleaseDate { get; set; }

        public List<int> SongsId { get; set; } = [];
        public virtual ICollection<Song> Songs { get; set; } = [];
    }
}
