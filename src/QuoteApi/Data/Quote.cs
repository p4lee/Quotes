using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace QuoteApi.Data

{
    public class Quote
    {
        public int Id { get; set; }
        public string TheQuote { get; set; }
        public string WhoSaid { get; set; }
        public DateTime WhenWasSaid { get; set; }
        public string QuoteCreator { get; set; }
        public string QuoteCreatorNormalized { get; set; }
        public DateTime QuoteCreateDate { get; set; }
    }
}
