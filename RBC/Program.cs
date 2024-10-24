using System.Globalization;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
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

static (MovieDto entryMovie, List<(Movie movie, double similarity)> recomendations) RbcRecommend(
    RbcContext context, int entryMovieId, WeightCombination weights, int recommendationNumber = 25)
{
    var entryMovieFromBd = context.Movies.Include(movie => movie.Genres).Include(movie => movie.Ratings)
        .Include(movie => movie.Tags).First(m => m.MovieId == entryMovieId);
    var entryMovie = new MovieDto(
        entryMovieFromBd.Title,
        entryMovieFromBd.Genres.Select(g => g.Name).ToList(),
        entryMovieFromBd.Ratings.Select(r => r.Score).ToList(),
        entryMovieFromBd.Tags.Select(t => t.Name).ToList());

    // Consultar todos os filmes do contexto
    var movies = context.Movies
        .Include(m => m.Genres)
        .Include(m => m.Ratings)
        .Include(m => m.Tags)
        .ToList();

    // Calcular similaridades
    var similarities = movies.Select(movie => (
        movie,
        similarity: CalculateOverallSimilarity(entryMovie, movie, weights)
    )).OrderByDescending(x => x.similarity).Take(recommendationNumber).ToList();
    
    similarities.Remove(similarities.First(x => x.movie.MovieId == entryMovieId));
    
    return (entryMovie, similarities);
}

static (MovieDto entryMovie, List<(Movie movie, double similarity)> recomendations) RbcRecommendByDto(
    RbcContext context, MovieDto entryMovie, WeightCombination weights, int recommendationNumber = 25)
{
    // Consultar todos os filmes do contexto
    var movies = context.Movies
        .Include(m => m.Genres)
        .Include(m => m.Ratings)
        .Include(m => m.Tags)
        .ToList();

    // Calcular similaridades
    var similarities = movies.Select(movie => (
        movie,
        similarity: CalculateOverallSimilarity(entryMovie, movie, weights)
    )).OrderByDescending(x => x.similarity).Take(recommendationNumber).ToList();
    
    return (entryMovie, similarities);
}

static double CalculateOverallSimilarity(MovieDto entry, Movie movie, WeightCombination weights)
{
    double genreSim = CalculateGenreSimilarity(
        entry.Genres.Select(g => g.ToLower()).ToHashSet(),
        movie.Genres.Select(g => g.Name.ToLower()).ToHashSet());
    double ratingSim = CalculateRatingSimilarity(entry.AverageRating(), movie.AverageRating());
    double popularitySim = CalculatePopularitySimilarity(entry.TotalRatings, movie.TotalRatings);
    double tagSim = CalculateTagSimilarity(
        entry.Tags.Select(t => t.ToLower()).ToList(),
        movie.Tags.Select(t => t.Name.ToLower()).ToList());

    return weights.Genres * genreSim +
           weights.Rating * ratingSim +
           weights.Popularity * popularitySim +
           weights.Tags * tagSim;
}

static double CalculateGenreSimilarity(HashSet<string> genres1, HashSet<string> genres2)
{
    int intersection = genres1.Intersect(genres2).Count();
    int union = genres1.Union(genres2).Count();
    return union != 0 ? (double)intersection / union : 0.0;
}

static double CalculateTagSimilarity(List<string> tags1, List<string> tags2)
{
    var set1 = tags1.ToHashSet();
    var set2 = tags2.ToHashSet();
    int intersection = set1.Intersect(set2).Count();
    int union = set1.Union(set2).Count();
    return union != 0 ? (double)intersection / union : 0.0;
}

static double CalculateRatingSimilarity(double rating1, double rating2)
    => 1 - Math.Abs(rating1 - rating2) / 5.0;

static double CalculatePopularitySimilarity(int pop1, int pop2)
    => pop1 != 0 && pop2 != 0 ? 1 - Math.Abs(pop1 - pop2) / (double)Math.Max(pop1, pop2) : 0.0;

var weights = new WeightCombination(0.3, 0.4, 0.2, 0.1);



app.MapGet("/movies", (int pageSize, int pageNumber) =>
    {
        return context.Movies.Skip(pageSize * (pageNumber - 1)).Take(pageSize).OrderBy(m => m.Ratings.Count);
    })
    .WithName("GetAllMovies")
    .WithOpenApi();

app.MapPost("/recommendations/{movieId}", (int movieId, int recommendationNumber = 25, WeightCombination? weightCombination = null) =>
    {
        var result = RbcRecommend(context, movieId, weightCombination ?? weights, recommendationNumber);
        var recommendations = result.recomendations.Take(recommendationNumber);
        return new
        {
            EntryMovie = new
            {
                MovieTitle = result.entryMovie.Title,
                Gengres = string.Join(", ", result.entryMovie.Genres),
                Ratings = string.Join(", ", result.entryMovie.Ratings.Select(r => r.ToString(CultureInfo.InvariantCulture))),
                AverageRating = result.entryMovie.AverageRating(),
                TotalRatings = result.entryMovie.TotalRatings,
                Tags = string.Join(", ", result.entryMovie.Tags)
            },
            WeightsUsed = weightCombination ?? weights,
            Recomendations = recommendations.Select(r =>
                new
                {
                    MovieTitle = r.movie.Title,
                    Gengres = string.Join(", ", r.movie.Genres.Select(g => g.Name)),
                    Ratings = string.Join(", ",
                        r.movie.Ratings.Select(r => r.Score.ToString(CultureInfo.InvariantCulture))),
                    AverageRating = r.movie.AverageRating(),
                    TotalRatings = r.movie.TotalRatings,
                    Tags = string.Join(", ", r.movie.Tags.Select(t => t.Name)),
                    Similarity = r.similarity
                })
        };
    })
    .WithName("GetRecommendationsByMovieId")
    .WithOpenApi();

#pragma warning disable ASP0020
app.MapPost("/recommendations-custom", (CustomFull customFull, int recommendationNumber = 25) =>
#pragma warning restore ASP0020
    {
        var result = RbcRecommendByDto(context, customFull.Movie, customFull.WeightCombination ?? weights);
        var recommendations = result.recomendations.Take(recommendationNumber).Take(recommendationNumber);
        return new
        {
            EntryMovie = new
            {
                MovieTitle = result.entryMovie.Title,
                Gengres = string.Join(", ", result.entryMovie.Genres),
                Ratings = string.Join(", ", result.entryMovie.Ratings.Select(r => r.ToString(CultureInfo.InvariantCulture))),
                AverageRating = result.entryMovie.AverageRating(),
                TotalRatings = result.entryMovie.TotalRatings,
                Tags = string.Join(", ", result.entryMovie.Tags)
            },
            WeightsUsed = customFull.WeightCombination ?? weights,
            Recomendations = recommendations.Select(r =>
                new
                {
                    MovieTitle = r.movie.Title,
                    Gengres = string.Join(", ", r.movie.Genres.Select(g => g.Name)),
                    Ratings = string.Join(", ",
                        r.movie.Ratings.Select(r => r.Score.ToString(CultureInfo.InvariantCulture))),
                    AverageRating = r.movie.AverageRating(),
                    TotalRatings = r.movie.TotalRatings,
                    Tags = string.Join(", ", r.movie.Tags.Select(t => t.Name)),
                    Similarity = r.similarity
                })
        };
    })
    .WithName("GetRecommendationsByMovieIdWithCustomWeightsAndCustomMovie")
    .WithOpenApi();

app.MapGet("/movies/{id}", async (int id) =>
    {
        var movie = await context.Movies
            .Include(m => m.Genres)
            .Include(movie => movie.Ratings)
            .Include(movie => movie.Tags)
            .FirstOrDefaultAsync(m => m.MovieId == id);

        return
            new CustomFull
            (
                new MovieDto
                (
                    movie!.Title,
                    movie.Genres.Select(g => g.Name).ToList(),
                    movie.Ratings.Select(r => r.Score).ToList(),
                    movie.Tags.Select(t => t.Name).ToList()
                ),
                new WeightCombination(0.3, 0.4, 0.2, 0.1)
            );
    })
    .WithName("GetMovieById")
    .WithOpenApi();

await app.RunAsync();
