namespace RBC.Models;

using System;
using CsvHelper.Configuration;

public class Tag
{
    public int MovieId { get; set; }
    public string TagValue { get; set; }
    public DateOnly Timestamp { get; set; }
    
    public override string ToString()
    {
        return $"MovieId: {MovieId}, Tag: {TagValue}, Timestamp: {Timestamp}";
    }
}



public sealed class TagMap : ClassMap<Tag>
{
    public TagMap()
    {
        Map(t => t.MovieId).Name("movieId");
        Map(t => t.TagValue).Name("tag");
        
        Map(t => t.Timestamp).Convert(row =>
        {
            var timestamp = long.Parse(row.Row.GetField("timestamp")!);
            return DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime);
        });
    }
}