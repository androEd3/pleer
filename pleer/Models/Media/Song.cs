using pleer.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pleer.Models.Media
{
    public class Song
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public int AlbumId { get; set; }
        public virtual Album Album { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Range(1, int.MaxValue)]
        public int? DurationSeconds { get; set; }
    }
}
