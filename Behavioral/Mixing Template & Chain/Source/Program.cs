using FinalChainOfResponsibility;

var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Build the Chain of Responsibility once. Each handler points at the next,
// so a Message walks down the chain until a link claims it. The order shows
// both flavours: single-name handlers and a multi-name handler.
IMessageHandler chain =
    new AlarmTriggeredHandler(
        new AlarmPausedHandler(
            new AlarmStoppedHandler(
                new SomeMultiHandler())));

// POST /messages/AlarmTriggered, /messages/Foo, /messages/Unknown?payload=...
app.MapPost("/messages/{name}", (string name, string? payload) =>
{
    chain.Handle(new Message(name, payload));
    return Results.Ok($"Dispatched '{name}' into the chain (see console output).");
})
.WithName("DispatchMessage");


app.Run();

