var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ExternalGreeter>();
builder.Services.AddSingleton<IGreeter, ExternalGreeterAdapter>();
var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", (IGreeter greeter) => greeter.Greet());


app.Run();
