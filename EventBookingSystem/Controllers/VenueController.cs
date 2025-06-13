using EventBookingSystem.Data;
using EventBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace EventBookingSystem.Controllers
{
    public class VenueController : Controller
    {
        private readonly AppDbContext _context;
        private readonly BlobStorageService _blobStorageService;
        private const string ContainerName = "images2";

        public VenueController(AppDbContext context, BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // GET: Venue
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venue.ToListAsync());
        }

        // GET: Venue/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venue/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("VenueId,VenueName,Location,Capacity")] Venue venue, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var blobName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                using (var stream = imageFile.OpenReadStream())
                {
                    await _blobStorageService.UploadFileAsync(ContainerName, blobName, stream);
                }
                venue.ImageUrl = $"https://st10477660imagesstorage.blob.core.windows.net/{ContainerName}/{blobName}";
            }
            _context.Add(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Venue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venue/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageUrl")] Venue venue, IFormFile imageFile)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Optionally delete the old image
                    if (!string.IsNullOrEmpty(venue.ImageUrl))
                    {
                        var uri = new Uri(venue.ImageUrl);
                        var oldBlobName = Path.GetFileName(uri.LocalPath);
                        await _blobStorageService.DeleteFileAsync(ContainerName, oldBlobName);
                    }

                    var blobName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                    using (var stream = imageFile.OpenReadStream())
                    {
                        await _blobStorageService.UploadFileAsync(ContainerName, blobName, stream);
                    }
                    venue.ImageUrl = $"https://st10477660imagesstorage.blob.core.windows.net/{ContainerName}/{blobName}";
                }

                _context.Update(venue);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VenueExists(venue.VenueId))
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

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            

            // Check for venue if it is associated with an event before delete
            if (venue != null)
            {
                var hasBookings = await _context.Booking.AnyAsync(b => b.VenueId == id);

                if (hasBookings)
                {
                    TempData["ErrorMessage"] = "Cannot delete this venue as it has active bookings.";
                    return View(venue);
                }

                var conflictingBooking = await _context.Event
                    .FirstOrDefaultAsync(b => b.VenueId == venue.VenueId);

                if (conflictingBooking != null)
                {
                    // Using TempData to pass the error message to the view
                    TempData["ErrorMessage"] = "Cannot delete a venue that is associated with an event";
                    return View(venue);
                }

                // Delete image from blob storage
                if (!string.IsNullOrEmpty(venue.ImageUrl))
                {
                    var uri = new Uri(venue.ImageUrl);
                    var blobName = Path.GetFileName(uri.LocalPath);
                    await _blobStorageService.DeleteFileAsync(ContainerName, blobName);
                }

                _context.Venue.Remove(venue);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueId == id);
        }
    }
}



 
