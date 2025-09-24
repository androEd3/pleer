using Microsoft.EntityFrameworkCore;
using pleer.Models.CONTEXT;
using pleer.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace pleer.Models.Media
{
    public class UserPlaylistsLink
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int PlaylistId { get; set; }
        public virtual Playlist Playlist { get; set; }
    }
}
