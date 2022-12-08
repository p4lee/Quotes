using Xunit;
using System;
using System.IO;
using System.Diagnostics;
using Xunit.Abstractions;
using QuoteApi.Data;
using Microsoft.EntityFrameworkCore;

namespace test;

public class UnitTest
{
    private readonly ITestOutputHelper output;
    public UnitTest(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async void Checkpoint01_1()
    {
        // check data folder
        string path = "../../../../../src/QuoteApi/Data";
        bool exists = Directory.Exists(path);
        Assert.True(exists);
        FileInfo fi = new FileInfo($"{path}/Quote.cs");
        Assert.NotNull(fi);
        Assert.True(fi.Exists);

        Quote q = new Quote();
        Assert.NotNull(q);
        Type t = q.GetType();
        Assert.NotNull(t);
        Assert.NotNull(t.GetProperty("WhoSaid"));
        Assert.NotNull(t.GetProperty("Id"));
    }

    [Fact]
    public async void Checkpoint01_2()
    {
        // check data folder
        string path = "../../../../../src/QuoteApi/Data";
        bool exists = Directory.Exists(path);
        Assert.True(exists);
        FileInfo fi = new FileInfo($"{path}/QuoteContext.cs");
        Assert.NotNull(fi);
        Assert.True(fi.Exists);
    }

    [Fact]
    public async void Checkpoint01_3()
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
}