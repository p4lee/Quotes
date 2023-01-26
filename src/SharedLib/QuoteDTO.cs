namespace SharedLib;

public class QuoteDTO
{
    public int Id { get; set; }
    public string Quote { get; set; }
    public string SaidBy { get; set; }

    public string When { get; set; }
    /*
    [JsonIgnore]
    public string QuoteCreator { get; set; }
    [JsonIgnore]
    public string QuoteCreatorNormalized { get; set; }
    [JsonIgnore]
    public DateTime QuoteCreateDate { get; set; }
    */

}
