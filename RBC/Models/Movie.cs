using CsvHelper.Configuration;
using CsvHelper.Configuration;
using System.Linq;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;

public class Movie()
{
    public int MovieId { get; set; }
    public string Title { get; set; }
    public List<string> Genres { get; set; } = [];

    public override string ToString()
    {
        return $"MovieId: {MovieId}, Title: {Title}, Genres: {string.Join(", ", Genres)}";
    }
}

public sealed class MovieMap : ClassMap<Movie>
{
    public MovieMap()
    {
        Map(m => m.MovieId).Name("movieId");
        Map(m => m.Title).Name("title");

        // Convert genres from a pipe-separated string into a list of strings
        Map(m => m.Genres).Convert(row =>
        {
            var genresString = row.Row.GetField(2); // Access field by index
            return new List<string>(genresString!.Split('|', System.StringSplitOptions.TrimEntries));
        });
    }
}