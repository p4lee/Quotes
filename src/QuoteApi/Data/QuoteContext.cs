using Microsoft.EntityFrameworkCore;
using QuoteApi.Data;
using System.Collections.Generic;

namespace QuoteApi.Data
{
    public class QuoteContext : DbContext
    {

        public QuoteContext(DbContextOptions<QuoteContext> options)
            : base(options) 
        { }

        public DbSet<Quote> Quotes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quote>().ToTable("Quotes");
            modelBuilder.Entity<Quote>().HasKey(q => q.Id);
            modelBuilder.Entity<Quote>().Property(q => q.TheQuote).IsRequired().HasMaxLength(1000);
            modelBuilder.Entity<Quote>().Property(q => q.WhoSaid).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Quote>().Property(q => q.WhenWasSaid).IsRequired();
            modelBuilder.Entity<Quote>().Property(q => q.QuoteCreator).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Quote>().Property(q => q.QuoteCreatorNormalized).IsRequired().HasMaxLength(100).HasComputedColumnSql("upper([QuoteCreator])");
            modelBuilder.Entity<Quote>().Property(q => q.QuoteCreateDate).IsRequired().HasDefaultValueSql("datetime('now')");
        }
    }

}
