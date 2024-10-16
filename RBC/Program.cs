var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/rbc", async () =>
    {
        var movies = await ReadMovieDataFromCsvAsync();
        return movies;
    })
    .WithName("teste")
    .WithOpenApi();

app.Run();

static async Task<List<Movie>> ReadMovieDataFromCsvAsync()
{
    var path = Path.Combine(Directory.GetCurrentDirectory(), "data.csv");
    var movies = new List<Movie>();

    using var reader = new StreamReader(path);

    await reader.ReadLineAsync();

    while (!reader.EndOfStream)
    {
        var line = await reader.ReadLineAsync();
        var values = line!.Split(',');
        var movie = new Movie
        {
            Name = values[0],
            Genre = values[1],
            Year = int.Parse(values[2]),
            ImdbRating = double.Parse(values[3]),
            ImdbVotes = int.Parse(values[4]),
            BoxOffice = int.Parse(values[5]),
            OscarWins = int.Parse(values[6]),
            OscarNominations = int.Parse(values[7])
        };

        movies.Add(movie);
    }

    return movies;
}