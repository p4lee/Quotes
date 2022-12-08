using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using QuoteApi.Data;
using SharedLib;
using Microsoft.Extensions.DependencyInjection;
using Test.Helpers;
using Xunit;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace test;

public class UnitTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<QuoteApi.Program> _applicationFactory;

    private readonly QuoteContext _db;

    public UnitTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _applicationFactory = new WebApplicationFactory<QuoteApi.Program>()
                .WithWebHostBuilder(builder =>
                {
                    // ... Configure test services
                });

        _client = _applicationFactory.CreateClient();

        string connectionstring = "Data Source=quotes.db";
        var optionsBuilder = new DbContextOptionsBuilder<QuoteContext>();
        optionsBuilder.UseSqlite(connectionstring);

        _db = new QuoteContext(optionsBuilder.Options);
    }

    [Fact]
    public async void Checkpoint02_0()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder<QuoteContext>();
        optionsBuilder.UseInMemoryDatabase("data");

        // Act
        var db = new QuoteContext(optionsBuilder.Options);

        // Assert
        Assert.NotNull(db);
        var quotes = await db.Quotes.ToListAsync();
        Assert.NotNull(quotes);
        Type t = typeof(QuoteContext);
        Assert.Equal(1, t.GetConstructors().Length);
    }

    [Fact]
    public async void Checkpoint02_1()
    {
        // Arrange
        System.Random rnd = new System.Random();
        string user = $"tester-{rnd.Next()}";
        var qText = $"quote-text-{rnd.Next()}";
        var qBy = $"by-{rnd.Next()}";
        var qTextMod = $"quote-text-mod-{rnd.Next()}";
        var quote = new JsonObject();
        quote.Add("id", 1);
        quote.Add("quote", qText);
        quote.Add("saidBy", qBy);
        quote.Add("when", DateTime.Now.ToString("O"));
        var mQuote = new JsonObject();
        mQuote.Add("id", 1);
        mQuote.Add("quote", qTextMod);
        mQuote.Add("saidBy", qBy);
        mQuote.Add("when", DateTime.Now.ToString("O"));

        // Act
        var response = await _client.PostAsJsonAsync($"/quotes/{user}", quote);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(response.Headers.Location);
        var location = response.Headers.Location;
        
        // Act 2
        var responseGet = await _client.GetFromJsonAsync<object>(location);
        Assert.NotNull(responseGet);
        JsonElement e = (JsonElement)responseGet;
        Assert.NotNull(e.GetProperty("id"));
        int qid = e.GetProperty("id").GetInt32();
        Assert.Equal(qText, e.GetProperty("quote").GetString());
        Assert.Equal(qBy, e.GetProperty("saidBy").GetString());
        
        // Act 3
        var responseGetTop5 = await _client.GetFromJsonAsync<IEnumerable<object>>($"/quotes");

        // Assert 3
        Assert.NotNull(responseGetTop5);
        var qIds = responseGetTop5.Select(r => ((JsonElement)r).GetProperty("id").GetInt32());
        Assert.Contains(qid, qIds);

        // Act 4
        var responseGet404 = await _client.GetAsync($"/quotes/d{user}/{qid}");

        // Assert 4
        Assert.NotNull(responseGet404);
        Assert.False(responseGet404.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, responseGet404.StatusCode);

        // Act 5
        var responseDelete404 = await _client.DeleteAsync($"/quotes/c{user}/{qid}");

        // Assert 5
        Assert.NotNull(responseDelete404);
        Assert.False(responseDelete404.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, responseDelete404.StatusCode);

        // Act 6
        var responsePut = await _client.PutAsJsonAsync($"/quotes/{user}/{qid}", mQuote);

        // Assert 6
        Assert.NotNull(responsePut);
        Assert.True(responsePut.IsSuccessStatusCode);

        // Act 7
        responseGet = await _client.GetFromJsonAsync<object>($"/quotes/{user}/{qid}");

        // Assert 7
        Assert.NotNull(responseGet);
        e = (JsonElement)responseGet;
        Assert.NotNull(e.GetProperty("id"));
        int mQid = e.GetProperty("id").GetInt32();
        Assert.Equal(qTextMod, e.GetProperty("quote").GetString());
        Assert.Equal(qBy, e.GetProperty("saidBy").GetString());
        Assert.Equal(qid, mQid);

        // Act 8
        var responseDelete = await _client.DeleteAsync($"/quotes/{user}/{qid}");

        // Assert 8
        Assert.NotNull(responseDelete);
        Assert.True(responseDelete.IsSuccessStatusCode);

        // Act 9
        responseGet404 = await _client.GetAsync($"/quotes/{user}/{qid}");

        // Assert 9
        Assert.NotNull(responseGet404);
        Assert.False(responseGet404.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, responseGet404.StatusCode);
    }

    [Fact]
    public async void Checkpoint02_2()
    {
        // Arrange

        // Act
        Type t = typeof(SharedLib.QuoteDTO);

        // Assert
        Assert.NotNull(t.GetProperty("Quote"));
        Assert.NotNull(t.GetProperty("Id"));
    }
}