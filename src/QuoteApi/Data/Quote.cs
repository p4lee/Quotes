using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace QuoteApi.Data

{
    public class Quote
    {
        public int Id { get; set; }
        public string TheQuote { get; set; }
        public string WhoSaid { get; set; }
        public DateTime WhenWasSaid { get; set; }
        [JsonIgnore]
        public string QuoteCreator { get; set; }
        [JsonIgnore]
        public string QuoteCreatorNormalized { get; set; }
        [JsonIgnore]
        public DateTime QuoteCreateDate { get; set; }
    }
}
