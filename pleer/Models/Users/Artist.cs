using pleer.Models.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pleer.Models.Users
{
    public class Artist
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MaxLength(64)] // Для хэша пароля (например, SHA256)
        public string PasswordHash { get; set; }

        [Column(TypeName = "date")]
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        //навигация
        public virtual ICollection<Album> ArtistsAlbums { get; set; } = new List<Album>();
    }
}
