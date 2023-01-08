using Microsoft.EntityFrameworkCore;

namespace QuoteApi.Data
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new QuoteContext(
                serviceProvider.GetRequiredService<DbContextOptions<QuoteContext>>()))
            {
                // Check if the database already exists
                if (context.Database.EnsureCreated())
                {
                    // If the database is new, add some seed data
                    context.Quotes.AddRange(
                        new Quote
                        {
                            TheQuote = "The only true wisdom is in knowing you know nothing.",
                            WhoSaid = "Socrates",
                            WhenWasSaid = new DateTime(400, 1, 1),
                            QuoteCreator = "John Doe",
                            QuoteCreatorNormalized = "JOHN DOE",
                            QuoteCreateDate = DateTime.Now
                        }
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}
