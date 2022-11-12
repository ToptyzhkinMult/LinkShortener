using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkShortener.Models;

namespace LinkShortener.Models
{
    public class AppContext : DbContext
    {
        public AppContext(DbContextOptions<AppContext> options) : base(options)
        {

        }
        public DbSet<LinkShortener.Models.Link> Link { get; set; }
    }
}
