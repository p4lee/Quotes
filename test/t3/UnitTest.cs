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
using Frontend;
using Frontend.Pages;
using Frontend.Shared;
using Frontend.Pages.Quotes;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;

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
    public async Task Checkpoint03_1()
    {
        // Arrange
        using var ctx = new TestContext();
        ctx.Services.AddSingleton<HttpClient>(_client);
        QuoteDTO quote = new QuoteDTO();
        string editUrl = "edit/x";
        string deleteUrl = "delete/y";

        // Act
        var cut = ctx.RenderComponent<QuoteItem>(parameters => parameters
            .Add(p => p.Quote, quote)
            .Add(p => p.EditLink, editUrl)
            .Add(p => p.DeleteLink, deleteUrl));
        var div = cut.Find("div.quote");
        var markup = cut.Markup;
        var linkHrefs = cut.FindAll("a").Select(l => l.GetAttribute("href"));

        // Assert
        Assert.NotNull(div);
        Assert.Contains(editUrl, linkHrefs);
        Assert.Contains(deleteUrl, linkHrefs);
    }

    [Fact]
    public async Task Checkpoint03_2()
    {
        // Arrange
        var testData = InitializeData(3, null, DateTime.MaxValue);
        await SendDataToApi(testData);
        using var ctx = new TestContext();
        ctx.Services.AddSingleton<HttpClient>(_client);

        // Act
        var cut = ctx.RenderComponent<Frontend.Pages.Index>();
        cut.WaitForState(() => cut.FindAll("div.quote").Count.Equals(5));
        var divs = cut.FindAll("div.quote");
        var markup = cut.Markup;

        // Assert
        Assert.NotNull(divs);
        foreach (var item in testData)
        {
            Assert.Contains(item["quote"], markup);
            Assert.Contains(item["saidBy"], markup);
        }
    }

    [Fact]
    public async Task Checkpoint03_3()
    {
        // Arrange
        System.Random rnd = new System.Random();
        int amount = 3;
        var testData = InitializeData(amount, $"user-{rnd.Next()}");
        await SendDataToApi(testData);
        using var ctx = new TestContext();
        ctx.Services.AddSingleton<HttpClient>(_client);

        // Act
        var cut = ctx.RenderComponent<QuotesList>(parameters => parameters
            .Add(p => p.User, testData[0]["user"]));

        cut.WaitForState(() => cut.FindAll("div.quote").Count.Equals(amount));
        var divs = cut.FindAll("div.quote");
        var h2 = cut.Find("h2");
        var markup = cut.Markup;

        // Assert
        Assert.NotNull(divs);
        Assert.Contains(testData[0]["user"], h2.TextContent);
        foreach (var item in testData)
        {
            Assert.Contains(item["quote"], markup);
            Assert.Contains(item["saidBy"], markup);
        }
    }

    [Fact]
    public async Task Checkpoint03_4()
    {
        // Arrange
        System.Random rnd = new System.Random();
        int amount = 1;
        var testData = InitializeData(amount, $"user-{rnd.Next()}");
        using var ctx = new TestContext();
        ctx.Services.AddSingleton<HttpClient>(_client);

        // Act
        var cut = ctx.RenderComponent<AddQuote>(parameters => parameters
            .Add(p => p.User, testData[0]["user"]));

        cut.WaitForState(() => cut.HasComponent<EditForm>());
        var markup = cut.Markup;
        var inputs = cut.FindAll("input");
        var submit = cut.Find("button[type=\"submit\"]");
        var h2 = cut.Find("h2");

        // Assert
        Assert.NotNull(inputs);
        Assert.Equal(3, inputs.Count);
        Assert.NotNull(submit);
        Assert.True(cut.HasComponent<InputText>());
        Assert.True(cut.HasComponent<InputDate<System.DateTime>>());
        Assert.Contains(testData[0]["user"], h2.TextContent);

        // Act 2
        cut.FindAll("input")[0].Change(testData[0]["quote"]);
        cut.FindAll("input")[1].Change(testData[0]["saidBy"]);
        cut.FindAll("input")[2].Change(testData[0]["when"]);

        await submit.ClickAsync(new MouseEventArgs());

        var responseGet = await _client.GetFromJsonAsync<IEnumerable<object>>($"/quotes/{testData[0]["user"]}");

        // Assert 3
        Assert.NotNull(responseGet);
        var elems = responseGet.Select(r => ((JsonElement)r));
        int? id = null;
        foreach (var item in elems)
        {
            if (item.GetProperty("quote").GetString() == testData[0]["quote"])
            {
                id = item.GetProperty("id").GetInt32();
                break;
            }
        }
        Assert.NotNull(id);

        // Act 3
        var cut2 = ctx.RenderComponent<EditQuote>(parameters => parameters
            .Add(p => p.User, testData[0]["user"])
            .Add(p => p.Id, id.Value));

        cut2.WaitForState(() => cut2.HasComponent<EditForm>());
        var markup2 = cut2.Markup;
        var inputs2 = cut2.FindAll("input");
        var submit2 = cut2.Find("button[type=\"submit\"]");
        var h2_2 = cut2.Find("h2");

        // Assert 3
        Assert.Contains(testData[0]["quote"], markup2);
        Assert.Contains(testData[0]["saidBy"], markup2);
        Assert.NotNull(inputs2);
        Assert.Equal(3, inputs2.Count);
        Assert.NotNull(submit2);
        Assert.True(cut2.HasComponent<InputText>());
        Assert.True(cut2.HasComponent<InputDate<System.DateTime>>());
        Assert.Contains(testData[0]["user"], h2_2.TextContent);
    }

    [Fact]
    public async Task Checkpoint03_5()
    {
        // Arrange
        System.Random rnd = new System.Random();
        int amount = 1;
        var testData = InitializeData(amount, $"user-{rnd.Next()}");
        await SendDataToApi(testData);

        // Act 1
        var responseGet = await _client.GetFromJsonAsync<IEnumerable<object>>($"/quotes/{testData[0]["user"]}");

        // Assert 1
        Assert.NotNull(responseGet);
        var elems = responseGet.Select(r => ((JsonElement)r));
        int? id = null;
        foreach (var item in elems)
        {
            if (item.GetProperty("quote").GetString() == testData[0]["quote"])
            {
                id = item.GetProperty("id").GetInt32();
                break;
            }
        }
        Assert.NotNull(id);

        using var ctx = new TestContext();
        ctx.Services.AddSingleton<HttpClient>(_client);

        // Act 2
        var cut = ctx.RenderComponent<DeleteQuote>(parameters => parameters
            .Add(p => p.User, testData[0]["user"])
            .Add(p => p.Id, id.Value));

        cut.WaitForState(() => cut.FindAll("button[type=\"submit\"]").Count.Equals(1));
        var markup = cut.Markup;
        var submit = cut.Find("button[type=\"submit\"]");
        var h2 = cut.Find("h2");

        // Assert 2
        Assert.NotNull(submit);
        Assert.Contains(testData[0]["user"], h2.TextContent);

        // Act 3
        await submit.ClickAsync(new MouseEventArgs());

        var responseGetDeleted = await _client.GetAsync($"/quotes/{testData[0]["user"]}/{id.Value}");

        // Assert 3
        Assert.NotNull(responseGetDeleted);
        Assert.False(responseGetDeleted.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, responseGetDeleted.StatusCode);
    }

    private List<Dictionary<string, string>> InitializeData(int amount, string? user = null, DateTime? date = null)
    {
        List<Dictionary<string, string>> testData = new List<Dictionary<string, string>>(amount);
        System.Random rnd = new System.Random();
        DateTime start = new DateTime(1900, 1, 1);
        int range = (DateTime.Today - start).Days;

        for (int i = 0; i < amount; i++)
        {
            testData.Add(
                new Dictionary<string, string> 
                {
                    {"user", user ?? $"tester-{rnd.Next()}"},
                    {"quote", $"quote-{rnd.Next()}"},
                    {"saidBy", $"by-{rnd.Next()}"},
                    {"when", date.HasValue ? date.Value.ToString("O") : start.AddDays(rnd.Next(range)).ToString("O")}
                }
            );
        }

        return testData;
    }

    private async Task SendDataToApi(List<Dictionary<string, string>> data)
    {
        foreach (var item in data)
        {
            var quote = new JsonObject();
            quote.Add("quote", item["quote"]);
            quote.Add("saidBy", item["saidBy"]);
            quote.Add("when", item["when"]);
            var response = await _client.PostAsJsonAsync($"/quotes/{item["user"]}", quote);        
        }
    }
}