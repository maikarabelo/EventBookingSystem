using EventBookingSystem.Data;
using EventBookingSystem.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;




namespace EventBookingSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Venue> Venue { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<Booking> Booking { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<EventType> EventType { get; set; }
    }
}



