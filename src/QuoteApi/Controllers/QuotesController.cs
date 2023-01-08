using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuoteApi.Data;
using SharedLib;
using System.Globalization;

namespace QuoteApi.Controllers
{
    [ApiController]
    [Route("/quotes")]

    public class QuotesController : ControllerBase
    {

        private readonly QuoteContext _context;

        public QuotesController(QuoteContext context)
        {
            _context = context;
        }

        
        // GET /quotes
        // returns top 5 latest quotes
        [HttpGet]
        public async Task<ActionResult<QuoteDTO[]>> GetTop5LatestQuotes()
        {

            var quotes = await _context.Quotes
                .OrderByDescending(q => q.QuoteCreateDate)
                .Take(5)
                .Select(q => new QuoteDTO
                {
                    Id = q.Id,
                    Quote = q.TheQuote,
                    SaidBy = q.WhoSaid,
                    When = q.WhenWasSaid.ToString("yyyy-MM-dd")
                })
                .ToArrayAsync();

            if (quotes == null || quotes.Length == 0)
            {
                return NotFound();
            }

            return quotes;
        }



        // GET /quotes/michael
        [HttpGet("{username}")]
        public async Task<ActionResult<QuoteDTO[]>> GetQuotesByUsername(string username)
        {
            var quotes = await _context.Quotes
                .Where(q => q.QuoteCreatorNormalized == username.ToUpper())
                .Select(q => new QuoteDTO
                {
                    Id = q.Id,
                    Quote = q.TheQuote,
                    SaidBy = q.WhoSaid,
                    When = q.WhenWasSaid.ToString("yyyy-MM-dd")
                })
                .ToArrayAsync();

            if (quotes == null || quotes.Length == 0)
            {
                return NotFound();
            }

            return quotes;
        }

        // GET /quotes/jane/3
        [HttpGet("{username}/{id}")]
        public async Task<ActionResult<QuoteDTO>> GetQuoteById(string username, int id)
        {
            // query the database to get the quote with the specified id and username
            var quote = await _context.Quotes
                .Where(q => q.QuoteCreatorNormalized == username.ToUpper() && q.Id == id)
                .FirstOrDefaultAsync();

            if (quote == null)
            {
                return NotFound(); // return 404 Not Found if the quote is not found
            }

            // map the quote to a QuoteDTO object and return it in the response
            var quoteDto = new QuoteDTO
            {
                Id = quote.Id,
                Quote = quote.TheQuote,
                SaidBy = quote.WhoSaid,
                When = quote.WhenWasSaid.ToString("yyyy-MM-dd") 
            };
            return Ok(quoteDto);
        }
        


        [HttpPost("{username}")]
        public async Task<ActionResult<QuoteDTO>> PostQuote(string username, QuoteDTO quoteDto)
        {
            // map the QuoteDTO object to a Quote entity
            DateTime dateTime = DateTime.Now;
            string isoDate = dateTime.ToString("yyyy-MM-dd");

            var quote = new Quote
            {
                TheQuote = quoteDto.Quote,
                WhoSaid = quoteDto.SaidBy,
                WhenWasSaid = DateTime.Parse(quoteDto.When), 
                QuoteCreator = username,
                QuoteCreatorNormalized = username.ToUpper(),
                QuoteCreateDate = DateTime.Parse(isoDate), 
            };

            // add the quote to the database
            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            // map the saved quote to a QuoteDTO object and return it in the response
            quoteDto.Id = quote.Id;
            return CreatedAtAction(nameof(GetQuoteById), new { username = username, id = quote.Id }, quoteDto);
        }



        // PUT: quotes/{username}/{id}
        // edits the quote with id {id} from user named {username}
        [HttpPut("{username}/{id}")]
        public async Task<IActionResult> PutQuote(string username, int id, QuoteDTO quoteDto)
        {
            // check if the quote with the specified id and username exists
            bool QuoteExists(string username, int id)
            {
                return _context.Quotes.Any(q => q.QuoteCreatorNormalized == username.ToUpper() && q.Id == id);
            }
            if (!QuoteExists(username, id))
            {
                return NotFound(); // return 404 Not Found if the quote is not found
            }

            // query the database to get the quote with the specified id and username
            var quote = await _context.Quotes
                .Where(q => q.QuoteCreatorNormalized == username.ToUpper() && q.Id == id)
                .FirstOrDefaultAsync();

            // update the quote properties with the values from the QuoteDTO object
            quote.TheQuote = quoteDto.Quote;
            quote.WhoSaid = quoteDto.SaidBy;
            quote.WhenWasSaid = DateTime.Parse(quoteDto.When); 

            // update the quote in the database
            _context.Quotes.Update(quote);
            await _context.SaveChangesAsync();

            return NoContent(); // return 204 No Content to indicate that the update was successful
        }


        //Delete quote with certain id from user

        [HttpDelete("{username}/{id}")]
        public async Task<ActionResult> DeleteQuote(string username, int id)
        {
            var quote = await _context.Quotes
                .Where(q => q.Id == id && q.QuoteCreatorNormalized == username.ToUpper())
                .SingleOrDefaultAsync();

            if (quote == null)
            {
                return NotFound();
            }

            _context.Quotes.Remove(quote);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

