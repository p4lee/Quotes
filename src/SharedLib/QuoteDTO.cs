
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace SharedLib;

public class QuoteDTO
{
    public int Id { get; set; }
    public string Quote { get; set; }
    public string SaidBy { get; set; }

    public DateTime When { get; set; }
    public string QuoteCreator { get; set; }
    public string QuoteCreatorNormalized { get; set; }
    public DateTime QuoteCreateDate { get; set; }


}
