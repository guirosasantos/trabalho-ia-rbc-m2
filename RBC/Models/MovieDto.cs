namespace RBC.Models;

public record MovieDto(string Title, List<string> Genres, ICollection<double> Ratings, ICollection<string> Tags)
{
    public double AverageRating()
    {
        if(Ratings.Count == 0)
            return 0;
        return Ratings.Average();
    }

    public int TotalRatings => Ratings.Count;
}