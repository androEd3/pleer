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
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MaxLength(64)]
        public string PasswordHash { get; set; }

        [Column(TypeName = "date")]
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        //навигация
        public virtual ICollection<UserPlaylistsLink> UserPlaylists { get; set; } = new List<UserPlaylistsLink>();
    }
}
