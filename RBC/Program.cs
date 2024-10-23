using System.Globalization;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using RBC;
using RBC.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

using var context = new RbcContext();

const string moviesPath = "Data/movies.csv";
const string ratingsPath = "Data/ratings.csv";
const string tagPath = "Data/tags.csv";

using var movieReader = new StreamReader(moviesPath);
using var csvMovies = new CsvReader(movieReader, CultureInfo.InvariantCulture);
csvMovies.Context.RegisterClassMap<MovieMap>();
var movies = csvMovies.GetRecords<Movie>().ToList();
context.Movies.AddRange(movies);
await context.SaveChangesAsync();

using var ratingReader = new StreamReader(ratingsPath);
using var csvRatings = new CsvReader(ratingReader, CultureInfo.InvariantCulture);
csvRatings.Context.RegisterClassMap<RatingMap>();

var filteredRatings = csvRatings.GetRecords<Rating>()
    .GroupBy(r => r.MovieId)
    .Select(g => g.First())
    .ToList();

context.Ratings.AddRange(filteredRatings);
await context.SaveChangesAsync();

using var tagReader = new StreamReader(tagPath);
using var csvTags = new CsvReader(tagReader, CultureInfo.InvariantCulture);
csvTags.Context.RegisterClassMap<TagMap>();
var tags = csvTags.GetRecords<Tag>().ToList();
context.Tags.AddRange(tags);

await context.SaveChangesAsync();
Console.WriteLine("Importação concluída!");


app.MapGet("/movies", (int PageSize, int PageNumber) =>
    {
        return context.Movies.Skip(PageSize * (PageNumber - 1)).Take(PageSize).OrderBy(m => m.Ratings.Count);
    })
    .WithName("GetAllMovies")
    .WithOpenApi();

app.MapGet("/tags", () =>
    {
        return context.Tags;
    })
    .WithName("GetAllTags")
    .WithOpenApi();

app.MapGet("/ratings", () =>
    {
        return context.Ratings;
    })
    .WithName("GetAllRatings")
    .WithOpenApi();

app.MapGet("/movies/{id}/ratings", (int id) =>
    {
        var movie = context.Movies
            .Include(m => m.Ratings)
            .FirstOrDefault(m => m.MovieId == id);

        return
            movie;
    })
    .WithName("GetRatingsByMovieId")
    .WithOpenApi();

await app.RunAsync();
