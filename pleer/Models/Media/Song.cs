using System.ComponentModel.DataAnnotations;

namespace pleer.Models.Media
{
    public class Song
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        public int AlbumId { get; set; }
        public virtual Album Album { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Required]
        public TimeSpan TotalDuration { get; set; }
    }
}
