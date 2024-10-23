using CsvHelper.Configuration;

namespace RBC.Models;

public class Movie
{
    public int MovieId { get; set; }
    public string Title { get; set; }
    public List<Genre> Genres { get; set; } = new();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public double AverageRating()
    {
        if(Ratings.Count == 0)
            return 0;
        return Ratings.Select(r => r.Score).Average();
    }

    public int TotalRatings => Ratings.Count;

    public override string ToString()
    {
        return $"MovieId: {MovieId}, Title: {Title}, Genres: {Genres.Select(g => g.ToString())}, Ratings: {Ratings.Select(r => r.ToString())}, Tags: {Tags.Select(t => t.ToString())}";
    }
}

public sealed class MovieMap : ClassMap<Movie>
{
    public MovieMap()
    {
        Map(m => m.MovieId).Name("movieId");
        Map(m => m.Title).Name("title");

        Map(m => m.Genres).Convert(row =>
        {
            var genresString = row.Row.GetField(2); // Acessa o campo de gêneros por índice
            return genresString.Split('|', StringSplitOptions.TrimEntries)
                .Select(name => new Genre { Name = name })
                .ToList();
        });
    }
}