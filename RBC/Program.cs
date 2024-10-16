using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using RBC.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

    var moviesPath = ("Data/movies.csv");
    List<Movie> movies;
    var ratingsPath = ("Data/ratings.csv");
    List<Rating> ratings;
    var tagPath = ("Data/tags.csv");
    List<Tag> tags;

    try
    {
        movies = ReadCsv<Movie>(moviesPath, new MovieMap());
        tags = ReadCsv<Tag>(tagPath, new TagMap());
        ratings = ReadCsv<Rating>(ratingsPath, new RatingMap());
        
        Console.WriteLine($"Loaded {movies.Count} movies.");
        Console.WriteLine($"Loaded {tags.Count} tags.");
        Console.WriteLine($"Loaded {ratings.Count} ratings.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    static List<T> ReadCsv<T>(string path, ClassMap<T> map) where T : class
    {
        using var reader = new StreamReader(path);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,  // Disable header validation
            MissingFieldFound = null // Ignore missing fields
        };
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap(map); // Register the specific map
        return new List<T>(csv.GetRecords<T>());
    }


    var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

