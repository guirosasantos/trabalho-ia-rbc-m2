namespace RBC.Models;

public record MovieDto
{
    public string Title { get; set; }
    public List<string> Genres { get; set; } = new();
    public ICollection<double> Ratings { get; set; } = new List<double>();
    public ICollection<string> Tags { get; set; } = new List<string>();
    
    public double AverageRating()
    {
        if(Ratings.Count == 0)
            return 0;
        return Ratings.Average();
    }

    public int TotalRatings => Ratings.Count;
}