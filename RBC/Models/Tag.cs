namespace RBC.Models;

using System;
using CsvHelper.Configuration;

public class Tag
{
    public int MovieId { get; set; }
    public string TagValue { get; set; }
    public DateTime Timestamp { get; set; }
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
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        });
    }
}