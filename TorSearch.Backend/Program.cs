using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using TorSearch.Backend.Data;
using TorSearch.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Register ElasticSearch service
builder.Services.AddSingleton<ElasticSearchService>(_ =>
    new ElasticSearchService(new Uri("http://elasticsearch:9200"), "tor_sites"));

var app = builder.Build();

// Save DomainInfo endpoint
app.MapPost("/saveDomainInfo", (DomainInfo domainInfo, ElasticSearchService elasticSearchService) =>
{
    return elasticSearchService.SaveDomainInfo(domainInfo) ? Results.Ok() : Results.BadRequest();
});

// Search endpoint
app.MapGet("/search", (string query, int page, ElasticSearchService elasticSearchService) =>
{
    var results = elasticSearchService.Search(query, page);
    return Results.Ok(results);
});

app.Run();
