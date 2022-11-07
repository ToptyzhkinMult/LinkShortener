using LinkShortener.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkShortener.Helper
{
    public class ShortLinkGenerator
    {
        private readonly MSSQLContext _context;

        public ShortLinkGenerator(MSSQLContext context)
        {
            _context = context;
        }
        public string GenerteShortLink()
        {
            string shortLinlk = "https://www.google.com/" + (new Random()).Next(1000, 9999);
            if (_context.Link.Any(l => l.ShortLink == shortLinlk)) shortLinlk = GenerteShortLink();

            return shortLinlk;
        }
    }
}
