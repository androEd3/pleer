using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pleer.Models
{
    public class Media
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Creator { get; set; }

        //song Uri
        public string SongPath { get; set; }
        //album cover Uri
        public string AlbumCoverPath { get; set; }
    }
}
