using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LinkShortener.Models;
using LinkShortener.Helper;
using System.Net;

namespace LinkShortener.Controllers
{
    public class LinkController : Controller
    {
        private readonly Models.AppContext _context;

        public LinkController(Models.AppContext context)
        {
            _context = context;
        }

        // GET: Link
        public async Task<IActionResult> Index()
        {
            return View(await _context.Link.ToListAsync());
        }

        // GET: Link/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var link = await _context.Link
                .FirstOrDefaultAsync(m => m.Id == id);
            if (link == null)
            {
                return NotFound();
            }

            return View(link);
        }

        // GET: Link/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Link/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OriginalLink,ShortLink,CreateDate,FolowingCount")] Link link)
        {
            if (!UrlIsValid(link.OriginalLink))
            {
                return View("Invalid", link.OriginalLink);
            }

            var request = HttpContext.Request;
            var host = request.Host.Value;
            var sheme = request.Scheme;

            link.ShortLink = (new ShortLinkGenerator(_context)).GenerteShortLink(sheme + "://" + host);
            link.CreateDate = DateTime.Now;
            if (ModelState.IsValid)
            {
                _context.Add(link);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(link);
        }

        public bool UrlIsValid(string url)
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                request.Timeout = 5000; //set the timeout to 5 seconds to keep the user from waiting too long for the page to load
                request.Method = "HEAD"; //Get only the header information -- no need to download any content

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    int statusCode = (int)response.StatusCode;
                    if (statusCode >= 100 && statusCode < 400) //Good requests
                    {
                        return true;
                    }
                    else if (statusCode >= 500 && statusCode <= 510) //Server Errors
                    {
                        //log.Warn(String.Format("The remote server has thrown an internal error. Url is not valid: {0}", url));

                        //Debug.WriteLine(String.Format("The remote server has thrown an internal error. Url is not valid: {0}", url));
                        Console.WriteLine(String.Format("The remote server has thrown an internal error. Url is not valid: {0}", url));
                        return false;
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError) //400 errors
                {
                    return false;
                }
                else
                {
                    //log.Warn(String.Format("Unhandled status [{0}] returned for url: {1}", ex.Status, url), ex);
                    Console.WriteLine(String.Format("Unhandled status [{0}] returned for url: {1}", ex.Status, url), ex);
                }
            }
            catch (Exception ex)
            {
                //log.Error(String.Format("Could not test url {0}.", url), ex);
                Console.WriteLine(String.Format("Could not test url {0}.", url), ex);

            }
            return false;
        }

        // GET: Link/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var link = await _context.Link.FindAsync(id);
            if (link == null)
            {
                return NotFound();
            }
            return View(link);
        }

        // POST: Link/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OriginalLink,ShortLink,CreateDate,FolowingCount")] Link link)
        {
            if (id != link.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(link);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LinkExists(link.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(link);
        }

        // GET: Link/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var link = await _context.Link
                .FirstOrDefaultAsync(m => m.Id == id);
            if (link == null)
            {
                return NotFound();
            }

            return View(link);
        }

        // POST: Link/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var link = await _context.Link.FindAsync(id);
            _context.Link.Remove(link);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LinkExists(int id)
        {
            return _context.Link.Any(e => e.Id == id);
        }

        public IActionResult ShortLinkRedirect(string shortLink)
        {
            Link link = _context.Link.FirstOrDefault(l => l.ShortLink == shortLink);
            link.FolowingCount += 1;
            _context.Update(link);
            _context.SaveChanges();
            return Redirect(link.OriginalLink);
        }

        [Route("Go/{inputLink}")]
        public IActionResult RedirectShortLink([FromRoute] string inputLink)
        {
            var request = HttpContext.Request;
            var host = request.Host.Value;
            var sheme = request.Scheme;
            var path = request.Path;

            string shortLink = sheme + "://" + host + path;
            //ShortLinkRedirect(sheme + "://" + host + path);
            
            Link link = _context.Link.FirstOrDefault(l => l.ShortLink == shortLink);

            if (link == null) return View("Invalid", shortLink);

            link.FolowingCount += 1;
            _context.Update(link);
            _context.SaveChanges();
            return Redirect(link.OriginalLink);
        }
    }
}
