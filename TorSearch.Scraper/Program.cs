using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TorSearch.Scraper;

Console.WriteLine("Hello, World! Starting to send sample data...");

// Sample data
var domainInfo = new DomainInfo
{
    Url = "https://example.com",
    Baseurl = "example.com",
    Title = "Example Title",
    Description = "This is a sample description.",
    Keywords = "sample, example, test",
    Html = "<html><head></head><body>Sample HTML content</body></html>",
    Date = DateTime.UtcNow
};

// Serialize the object to JSON
string jsonString = JsonSerializer.Serialize(domainInfo);

// HttpClient to send the request
using var client = new HttpClient();
var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

try
{
    // Replace with the actual URL of your backend
    string backendUrl = "http://backend:80/saveDomainInfo";
    var response = await client.PostAsync(backendUrl, content);

    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Data sent successfully.");
    }
    else
    {
        Console.WriteLine($"Failed to send data. Status code: {response.StatusCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Exception occurred: {ex.Message}");
}
