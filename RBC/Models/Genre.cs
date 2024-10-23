namespace RBC.Models;

public class Genre
{
    public int GenreId { get; set; }
    public int MovieId { get; set; }
    public string Name { get; set; }
    
    public override string ToString()
    {
        return $"Name: {Name}";
    }
}