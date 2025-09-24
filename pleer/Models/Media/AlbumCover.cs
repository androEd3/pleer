using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pleer.Models.Media
{
    public class AlbumCover
    {
        public int Id { get; set; }

        [Required]
        public string FilePath { get; set; }
    }
}
