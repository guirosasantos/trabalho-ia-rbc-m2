namespace RBC.Models;

using System;
using CsvHelper.Configuration;

public class Rating
{
    public DateTimeOffset Timestamp { get; set; }
    public int MovieId { get; set; }
    public double Score { get; set; }
    // public Movie Movie { get; set; }

    public override string ToString()
    {
        return $"MovieId: {MovieId}, Score: {Score}, Timestamp: {Timestamp}";
    }
}

public sealed class RatingMap : ClassMap<Rating>
{
    public RatingMap()
    {
        Map(r => r.MovieId).Name("movieId");
        Map(r => r.Score).Name("rating");

        Map(r => r.Timestamp).Convert(row =>
        {
            var timestamp = long.Parse(row.Row.GetField("timestamp"));
            return DateTimeOffset.FromUnixTimeSeconds(timestamp);
        });
    }
}