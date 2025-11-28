using System.ComponentModel.DataAnnotations;

namespace pleer.Models.Media
{
    public class Song
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        //ban status
        public bool Status { get; set; }

        public int AlbumId { get; set; }
        public virtual Album Album { get; set; }

        [Required]
        public string FilePath { get; set; }

        public TimeSpan TotalDuration { get; set; }

        // kolichestvo poroslushivaniy
        public int TotalPlays { get; set; }
    }
}
