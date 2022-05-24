using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace ExampleApi;

public class MovieControllerTests
{
    private readonly HttpClient _client; // HttpClient acts as a session to send HTTP requests (CRUD operations)

    private readonly List<Movie> _listOfMovies = new List<Movie>
        {new Movie {Title = "Casablanca", Director = "Michael Curtiz", Year = 1944}};

    public MovieControllerTests()
    {
        // Create mocks of the service layer
        // We are testing the controller class but still need to return values from our service layer for the tests to work
        var mockMovieService = new Mock<IMovieService>();
        mockMovieService.Setup(u => u.AddMovie(It.IsAny<Movie>())).ReturnsAsync(1); // Returns an int value expressing the number of entries added (ex. Task<Int32>)
        mockMovieService.Setup(u => u.GetAllMovies()).ReturnsAsync(_listOfMovies);
        
        // Create application using WebApplicationFactory; creates an instance of TestServer using our Program (or Startup) class as an entry point
        _client = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => // Creates a new WebApplicationFactory with a IWebHostBuilder
            { 
                builder.ConfigureServices(services =>
                {
                    // Mock the services
                    services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IMovieService))!) // Get rid of the actual service being used;
                    services.AddSingleton(mockMovieService.Object); // Substitute it with the mock service. "AddSingleton" just specifies that we're adding a single service
                }); 
            })
            .CreateClient(); // Creates an instance of HttpClient using our configuration
    }
    
    [Fact]
    public async Task HttpGet_RequestReceived_ReturnsOk()
    {
        HttpResponseMessage response = await _client.GetAsync("/movies");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Convert _listOfMovies into a stringified JSON to compare against what is being returned from the endpoint
        var expected = JsonConvert.SerializeObject(_listOfMovies,
            new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});
        Assert.Equal(expected, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task HttpPost_RequestReceived_ReturnsOk()
    {
        Movie movie = new() {Title = "Citizen Kane", Director = "Orson Welles", Year = 1941};
        StringContent userJson = new(JsonConvert.SerializeObject(movie), Encoding.UTF8, "application/json"); // Create a JSON of 'movie' to be added
        HttpResponseMessage response = await _client.PostAsync("/movies", userJson);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(userJson, await response.Content.ReadAsStringAsync());
    }
}