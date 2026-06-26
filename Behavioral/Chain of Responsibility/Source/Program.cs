var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapPost("/request", (Request request) =>
{
    // Build the chain: Manager -> Director -> CEO
    var manager = new ManagerHandler();
    var director = new DirectorHandler();
    var ceo = new CEOHandler();

    manager.SetNext(director).SetNext(ceo);

    var result = manager.Handle(request);

    return Results.Ok(new { request.UserRole, request.Amount, Result = result });
});

app.Run();
