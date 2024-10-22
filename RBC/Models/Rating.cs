namespace RBC.Models;

using System;
using CsvHelper.Configuration;

public class Rating
{
    public int MovieId { get; set; }
    public float RatingValue { get; set; }
    public DateOnly Timestamp { get; set; }
    
    public override string ToString()
    {
        return $"MovieId: {MovieId}, Rating: {RatingValue}, Timestamp: {Timestamp}";
    }
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
            return DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime);
        });
    }
}