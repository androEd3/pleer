using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pleer.Models.Media
{
    public class Album
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string ArtistId { get; set; }

        //metadata
        public DateOnly ReleazeDate { get; set; }
        //album cover Uri
        public int AlbumCoverId { get; set; }

        //songs list in album
        public List<int> Songs = new List<int>();
    }
}
