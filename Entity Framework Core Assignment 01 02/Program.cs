/*  assignment1 
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Entity_Framework_Core_Assignment_01_02
{

   public class Book
   {
       public int Id { get; set; }

       public string Title { get; set; }

       public string ISBN { get; set; }

       public decimal Price { get; set; }

       public int Pages { get; set; }

       public int PublishedYear { get; set; }

       public bool InStock { get; set; }

       public int AuthorId { get; set; }
       public Author Author { get; set; }

       public int CategoryId { get; set; }
       public Category Category { get; set; }
   }

   public class Author
   {
       public int Id { get; set; }

       public string FirstName { get; set; }

       public string LastName { get; set; }

       public string Email { get; set; }

       public string Biography { get; set; }

       public DateTime DateOfBirth { get; set; }

       public ICollection<Book> Books { get; set; }
           = new List<Book>();
   }

   public class Category
   {
       public int Id { get; set; }

       public string Name { get; set; }

       public string Description { get; set; }

       public bool IsActive { get; set; }

       public ICollection<Book> Books { get; set; }
           = new List<Book>();
   }

   public class BookStoreContext : DbContext
   {
       public DbSet<Book> Books { get; set; }

       public DbSet<Author> Authors { get; set; }

       public DbSet<Category> Categories { get; set; }

       protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
       {
           optionsBuilder.UseSqlServer(
               "Server=.;Database=BookStoreDb;Trusted_Connection=True;TrustServerCertificate=True");
       }
   }
   */
using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    namespace EventHub
    {
        // ATTENDEE (DATA ANNOTATIONS ONLY)
        public class Attendee
        {
            [Key]
            public int Id { get; set; }

            [Required, MaxLength(100)]
            public string FullName { get; set; }

            [Required]
            public string Email { get; set; }

            public Address Address { get; set; }

            public Badge Badge { get; set; }

            public ICollection<Registration> Registrations { get; set; }
                = new List<Registration>();
        }

        // Owned Type
        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
            public string PostalCode { get; set; }
        }

        // ORGANIZER (SEPARATE CONFIG)
        public class Organizer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsVerified { get; set; }

            public OrganizerProfile Profile { get; set; }

            public ICollection<Event> Events { get; set; }
                = new List<Event>();
        }

        public class OrganizerProfile
        {
            public int Id { get; set; }
            public string Bio { get; set; }
            public string Website { get; set; }
            public string LogoUrl { get; set; }

            public int OrganizerId { get; set; }
            public Organizer Organizer { get; set; }
        }

        // BADGE (SEPARATE CONFIG)
        public class Badge
        {
            public int Id { get; set; }
            public string BadgeNumber { get; set; }
            public DateTime IssuedAt { get; set; }
            public string Tier { get; set; }

            public int AttendeeId { get; set; }
            public Attendee Attendee { get; set; }
        }

        // REGISTRATION (SEPARATE CONFIG)
        public class Registration
        {
            public int AttendeeId { get; set; }
            public Attendee Attendee { get; set; }

            public int EventId { get; set; }
            public Event Event { get; set; }

            public string Note { get; set; }
            public DateTime RegisteredAt { get; set; }
        }

        // EVENT (FLUENT API ONLY)
        public class Event
        {
            public int Id { get; set; }

            public string Title { get; set; }
            public string Description { get; set; }

            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public int MaxAttendees { get; set; }

            public int OrganizerId { get; set; }
            public Organizer Organizer { get; set; }

            public int? ParentEventId { get; set; }
            public Event ParentEvent { get; set; }

            public ICollection<Event> Sessions { get; set; }
                = new List<Event>();

            public ICollection<Registration> Registrations { get; set; }
                = new List<Registration>();
        }

        // DB CONTEXT
        public class EventHubContext : DbContext
        {
            public DbSet<Organizer> Organizers { get; set; }
            public DbSet<OrganizerProfile> OrganizerProfiles { get; set; }
            public DbSet<Event> Events { get; set; }
            public DbSet<Attendee> Attendees { get; set; }
            public DbSet<Badge> Badges { get; set; }
            public DbSet<Registration> Registrations { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(
                    "Server=.;Database=EventHubDb;Trusted_Connection=True;TrustServerCertificate=True");
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                // EVENT (FLUENT API ONLY) 
                modelBuilder.Entity<Event>(entity =>
                {
                    entity.HasKey(e => e.Id);

                    entity.Property(e => e.Title)
                          .IsRequired()
                          .HasMaxLength(200);

                    entity.Property(e => e.Description)
                          .HasMaxLength(2000);

                    entity.HasOne(e => e.Organizer)
                          .WithMany(o => o.Events)
                          .HasForeignKey(e => e.OrganizerId);

                    entity.HasOne(e => e.ParentEvent)
                          .WithMany(e => e.Sessions)
                          .HasForeignKey(e => e.ParentEventId)
                          .OnDelete(DeleteBehavior.Restrict);

                    entity.HasMany(e => e.Registrations)
                          .WithOne(r => r.Event)
                          .HasForeignKey(r => r.EventId);

                    // Shadow properties (timestamps)
                    entity.Property<DateTime>("CreatedAt")
                          .HasDefaultValueSql("GETDATE()");

                    entity.Property<DateTime>("LastModifiedAt")
                          .HasDefaultValueSql("GETDATE()");
                });

                // ORGANIZER CONFIG 
                modelBuilder.Entity<Organizer>(entity =>
                {
                    entity.HasKey(o => o.Id);

                    entity.Property(o => o.Name)
                          .IsRequired()
                          .HasMaxLength(150);

                    entity.HasOne(o => o.Profile)
                          .WithOne(p => p.Organizer)
                          .HasForeignKey<OrganizerProfile>(p => p.OrganizerId);
                });

                // ORGANIZER PROFILE 
                modelBuilder.Entity<OrganizerProfile>(entity =>
                {
                    entity.HasKey(p => p.Id);

                    entity.Property(p => p.Bio).HasMaxLength(1000);
                    entity.Property(p => p.Website).HasMaxLength(200);
                    entity.Property(p => p.LogoUrl).HasMaxLength(300);
                });

                //  BADGE 
                modelBuilder.Entity<Badge>(entity =>
                {
                    entity.HasKey(b => b.Id);

                    entity.Property(b => b.BadgeNumber).IsRequired();

                    entity.Property(b => b.Tier)
                          .IsRequired()
                          .HasMaxLength(20);

                    entity.HasOne(b => b.Attendee)
                          .WithOne(a => a.Badge)
                          .HasForeignKey<Badge>(b => b.AttendeeId);
                });

                //  REGISTRATION 
                modelBuilder.Entity<Registration>(entity =>
                {
                    entity.HasKey(r => new { r.AttendeeId, r.EventId });

                    entity.Property(r => r.Note)
                          .HasMaxLength(500);

                    entity.Property(r => r.RegisteredAt)
                          .HasDefaultValueSql("GETDATE()");
                });
            }
        }

        internal class Program
        {
            static void Main(string[] args)
            {
                using var db = new EventHubContext();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                Console.WriteLine("EventHub Database Created Successfully");
            }
        }
}
/* assignment1
    internal class Program
    {
        static void Main(string[] args)
        {
            
            using var db = new BookStoreContext();

            db.Database.Migrate();

            Console.WriteLine("Database Created Successfully");
        
            }
    }
}
*/