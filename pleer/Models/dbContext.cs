using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pleer.Models
{
    public class dbContext : DbContext
    {
        public dbContext() : base()
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<Media> Media { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=ROONIN-COMPUTAH\\SQLEXPRESS;Database=pleer;Integrated Security=SSPI;Trust Server Certificate=True");
            }
        }
    }
}
