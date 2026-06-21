public class ExternalGreeterAdapter : IGreeter
{
    private readonly ExternalGreeter _externalGreeter;

    public ExternalGreeterAdapter(ExternalGreeter externalGreeter)
    {
        _externalGreeter = externalGreeter ?? throw new ArgumentNullException(nameof(externalGreeter));

    }
    public string Greet()
    {
        // For demonstration, we can use a fixed name or make it dynamic as needed
        string name = "Almansoor";
        return _externalGreeter.GetGreeting(name);
    }
}