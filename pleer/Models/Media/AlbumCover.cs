using System.ComponentModel.DataAnnotations;

namespace pleer.Models.Media
{
    public class AlbumCover
    {
        public int Id { get; set; }

        [Required]
        public string FilePath { get; set; }
    }
}
