using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventBookingSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using EventBookingSystem.Data;

public class BookingController : Controller
{
    private readonly AppDbContext _context;

    public BookingController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Bookings - Display list of bookings
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        var bookingsQuery = _context.Booking
            .Include(b => b.Event)
            .Include(b => b.Venue)
            .Include(b => b.Customer)
            .Include(b => b.Event.EventType)
            .AsQueryable();

        if (startDate.HasValue && endDate.HasValue)
        {
            bookingsQuery = bookingsQuery
                .Where(b => b.BookingDate.Date >= startDate.Value.Date && b.BookingDate.Date <= endDate.Value.Date);
        }

        var bookings = await bookingsQuery.ToListAsync();
        return View(bookings);
    }

    //Search bookings
    public async Task<IActionResult> SearchBookings(string searchQuery)
    {
        var results = await _context.Booking
            .Include(b => b.Venue)
            .Include(b => b.Event)
            .Include(b => b.Customer)
            .Include(b => b.Event.EventType)
            .Where(b => b.BookingId.ToString().Contains(searchQuery) ||
                        b.Event.EventName.Contains(searchQuery) ||
                        b.Venue.VenueName.Contains(searchQuery) ||
                        b.Customer.FullName.Contains(searchQuery) ||
                        b.Event.EventType.Name.Contains(searchQuery))
            .ToListAsync();

        return View(results);
    }


    // GET: Create - Show form to create a new booking
    public IActionResult Create()
    {
        ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName");
        ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName");
        ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "FullName"); // Fixed Customer dropdown
        return View();
    }

    // POST: Create - Handle form submission to create a new booking
    [HttpPost]
    public async Task<IActionResult> Create([Bind("BookingId,EventId,VenueId,CustomerId,BookingDate")] Booking booking)
    {
        // Check for booking conflicts Event Create
        var conflictingBooking = await _context.Event
            .Where(b => b.VenueId == booking.VenueId && b.EventDate.Date == booking.BookingDate.Date)
            .FirstOrDefaultAsync();

        // Check for booking same venue on the same date conflicts Create
        var conflictingBookingVenue = await _context.Booking
            .Include(b => b.Event)
            .FirstOrDefaultAsync(b => b.VenueId == booking.VenueId && b.BookingDate.Date == booking.BookingDate.Date && b.BookingId != booking.BookingId);

        // Check for booking conflicts same customer same date Create
        var conflictingBookingCustomer = await _context.Booking
            .Where(b => b.CustomerId == booking.CustomerId && b.BookingDate.Date == booking.BookingDate.Date)
            .FirstOrDefaultAsync();

        if (conflictingBooking != null)
        {
            // Using TempData to pass the error message to the view
            TempData["ErrorMessage"] = "There's an event already happening on the day you are trying to book the venue!";
            ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName");
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName");
            ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "FullName");
            return View(booking);
        }

        if (conflictingBookingVenue != null)
        {
            // Using TempData to pass the error message to the view
            TempData["ErrorMessage"] = "This venue is already booked for the selected date and time!";
            ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName");
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName");
            ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "FullName");
            return View(booking);
        }

        if (conflictingBookingCustomer != null)
        {
            // Using TempData to pass the error message to the view
            TempData["ErrorMessage"] = "Customer cannot have multiple bookings for the same date!";
            ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName");
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName");
            ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "FullName");
            return View(booking);
        }

        // create booking if no conflicts
        _context.Add(booking);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Edit - Display form to edit an existing booking
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _context.Booking
            .Include(b => b.Event)
            .Include(b => b.Venue)
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return NotFound();
        }

        // Populate dropdowns
        ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
        ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
        ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "FullName", booking.CustomerId);

        return View(booking);
    }
    // POST: Edit - Handle form submission to update an existing booking
    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,CustomerId,BookingDate")] Booking booking)
    {
        if (id != booking.BookingId)
        {
            return NotFound();
        }

        try
        {
            // Check for booking conflicts Event after editing
            var conflictingBooking = await _context.Event
                .Where(b => b.VenueId == booking.VenueId && b.EventDate.Date == booking.BookingDate.Date)
                .FirstOrDefaultAsync();

            // Check for booking same venue on the same date conflicts after editing
            var conflictingBookingVenue = await _context.Booking
                .Include(b => b.Event)
                .FirstOrDefaultAsync(b => b.VenueId == booking.VenueId && b.BookingDate.Date == booking.BookingDate.Date && b.BookingId != booking.BookingId);

            // Check for booking conflicts same customer same date after editing
            var conflictingBookingCustomer = await _context.Booking
                .Where(b => b.CustomerId == booking.CustomerId && b.BookingDate.Date == booking.BookingDate.Date)
                .FirstOrDefaultAsync();

            if (conflictingBooking != null)
            {
                // Using TempData to pass the error message to the view
                TempData["ErrorMessage"] = "There's an event happening in the day you are trying to book the venue!";
                ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName");
                ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName");
                ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "FullName");
                return View(booking);
            }


            if (conflictingBookingVenue != null)
            {
                // Using TempData to pass the error message to the view
                TempData["ErrorMessage"] = "This venue is already booked for the selected date and time!";
                ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName");
                ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName");
                ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "FullName");
                return View(booking);
            }

            if (conflictingBookingCustomer != null)
            {
                // Using TempData to pass the error message to the view
                TempData["ErrorMessage"] = "Customer cannot have multiple bookings for the same date!";
                ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName");
                ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName");
                ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "FullName");
                return View(booking);
            }

            // Update booking if no conflicts
            _context.Update(booking);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BookingExists(booking.BookingId))
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

    // Delete: Show form to confirm deletion of booking
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _context.Booking
            .Include(b => b.Event)
            .Include(b => b.Venue)
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    // Delete: Handle booking deletion
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var booking = await _context.Booking.FindAsync(id);

        _context.Booking.Remove(booking);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool BookingExists(int id)
    {
        return _context.Booking.Any(b => b.BookingId == id);
    }

    // Helper to populate dropdowns for Create/Edit views
    private void PopulateDropdowns(Booking booking)
    {
        ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
        ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
        ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "FullName", booking.CustomerId);
    }
}