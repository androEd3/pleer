using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pleer.Models.Media
{
    public class Song
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Artist { get; set; }

        //song path
        public string SongPath { get; set; }
        public int AlbumCoverId { get; set; }

        //album id
        public int AlbumId { get; set; }
    }
}
