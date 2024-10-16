public sealed class Movie
{
    public string Name { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int Year { get; set; }
    public double ImdbRating { get; set; }
    public int ImdbVotes { get; set; }
    public int BoxOffice { get; set; }
    public int OscarWins { get; set; }
    public int OscarNominations { get; set; }
}