namespace RBC.Models;

using CsvHelper.Configuration;

public class Tag
{
    public int TagId { get; set; }
    public int MovieId { get; set; }
    public string Name { get; set; }
    // public Movie Movie { get; set; }

    public override string ToString()
    {
        return $"Tag: {Name}, MovieId: {MovieId}";
    }
}

public sealed class TagMap : ClassMap<Tag>
{
    public TagMap()
    {
        // Map(t => t.TagId).Name("tagId");
        Map(t => t.MovieId).Name("movieId");
        Map(t => t.Name).Name("tag");
    }
}