using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pleer.Models.CONTEXT
{
    public static class InitilizeData
    {
        public static void SeedData()
        {
            //manage db
            dbContext _context = new dbContext();
            _context.Database.EnsureCreated();

            //Seed songs


            //Seed albums


            //Seed artist


        }
    }
}
