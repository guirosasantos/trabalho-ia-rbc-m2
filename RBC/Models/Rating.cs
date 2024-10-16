namespace RBC.Models;

using System;
using CsvHelper.Configuration;

public class Rating
{
    public int MovieId { get; set; }
    public float RatingValue { get; set; }
    public DateTime Timestamp { get; set; }
}

public sealed class RatingMap : ClassMap<Rating>
{
    public RatingMap()
    {
        Map(r => r.MovieId).Name("movieId");
        Map(r => r.RatingValue).Name("rating");
        

        Map(r => r.Timestamp).Convert(row =>
        {
            var timestamp = long.Parse(row.Row.GetField("timestamp"));
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        });
    }
}