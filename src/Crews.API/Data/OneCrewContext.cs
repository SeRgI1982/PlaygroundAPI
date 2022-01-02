using System;
using Crews.API.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Crews.API.Data
{
    public class OneCrewContext : IdentityDbContext<User, Role, int>
    {
        private const string DatabaseName = "onecrew.db";

        public OneCrewContext(DbContextOptions<OneCrewContext> options) : base(options)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DatabasePath = System.IO.Path.Join(path, DatabaseName);
        }

        public string DatabasePath { get; }

        public DbSet<Crew> Crews { get; set; }
        
        public DbSet<Training> Trainings { get; set; }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DatabasePath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map table names
            modelBuilder.Entity<Crew>().ToTable("Crews", "dbo");
            modelBuilder.Entity<Crew>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(nameof(Crew.Name), nameof(Crew.ShortName), nameof(Crew.Group), nameof(Crew.Year)).IsUnique();
                entity.Property(e => e.DateTimeAdd).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Training>().ToTable("Trainings", "dbo");
            modelBuilder.Entity<Training>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DateTimeAdd).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
