using System;
using System.Diagnostics;
using EventBookingSystem.Data;
using EventBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace EventBookingSystem.Controllers
{
    public class EventController : Controller
    {
        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Event.Include(e => e.Venue).Include(e => e.EventType).ToListAsync();


            return View(events);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName");
            ViewData["EventTypeId"] = new SelectList(_context.EventType, "EventTypeId", "Name");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("EventId,EventName,EventDate,Description,VenueId,EventTypeId")] Event @event)
        {

            _context.Add(@event);
            await _context.SaveChangesAsync(); 
            return RedirectToAction(nameof(Index));
        }


        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            // Populate the venue dropdown for the edit form
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName", @event.VenueId);
            ViewData["EventTypeId"] = new SelectList(_context.EventType, "EventTypeId", "Name", @event.EventTypeId);
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId, EventName, EventDate, Description, VenueId,EventTypeId")] Event @event)
        {
            if (id != @event.EventId)
            {
                return NotFound();
            }

            try
            {
                _context.Update(@event);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(@event.EventId))
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

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Event.FindAsync(id);

            // If event is not found, return to Index or handle appropriately
            if (@event == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check for event if it is associated with booking before delete
            if (@event != null)
            {
                var conflictingBooking = await _context.Booking
                    .FirstOrDefaultAsync(b => b.EventId == @event.EventId);

                if (conflictingBooking != null)
                {
                    // Using TempData to pass the error message to the view
                    TempData["ErrorMessage"] = "Cannot delete an event that is associated with an booking";
                    ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName", @event.VenueId);
                    ViewData["EventTypeId"] = new SelectList(_context.EventType, "EventTypeId", "Name", @event.EventTypeId);

                    return View(@event);
                }
            }

            if (@event != null)
            {
                _context.Event.Remove(@event);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if an event exists
        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventId == id);
        }
    }
}
