using TemplateMethod;
var builder = WebApplication.CreateBuilder(args);
builder.Services
 .AddSingleton<SearchMachine>(x
        => new LinearSearchMachine(1, 10, 5, 2, 123, 333, 4))
    .AddSingleton<SearchMachine>(x
        => new BinarySearchMachine(1, 2, 3, 4, 5, 6, 7, 8, 9, 10))
;
var app = builder.Build();
app.MapGet("/search/{number}", SearchForIndex);
app.Run();
IEnumerable<SearchResult> SearchForIndex(
    int number, IEnumerable<SearchMachine> searchMachines)
{
    foreach (var searchMachine in searchMachines)
    {
        var name = searchMachine.GetType().Name;
        var index = searchMachine.IndexOf(number);
        var found = index.HasValue;
        yield return new SearchResult(number, name, found, index);
    }
}
public record class SearchResult(
    int SearchedNumber,
    string Name,
    bool Found,
    int? Index
);