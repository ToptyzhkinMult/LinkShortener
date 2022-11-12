using LinkShortener.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkShortener.Helper
{
    public class ShortLinkGenerator
    {
        private readonly Models.AppContext _context;

        public ShortLinkGenerator(Models.AppContext context)
        {
            _context = context;
        }
        public string GenerteShortLink(string headLink)
        {
            string shortLinlk = headLink + "/" + "Go" + "/" + RandomGenerate(10);
            if (_context.Link.Any(l => l.ShortLink == shortLinlk)) shortLinlk = GenerteShortLink(headLink);

            return shortLinlk;
        }

        private string RandomGenerate(int length)
        {
            Random random = new Random();
            string character = "0123456789qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
            char[] letters = character.ToCharArray();
            string s = "";
            for (int i = 0; i < length; i++)
            {
                s += letters[random.Next(letters.Length)].ToString();
            }
            return s;
        }
    }
}
