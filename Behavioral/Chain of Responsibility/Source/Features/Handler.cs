public abstract class Handler : IHandler
{
    private IHandler? _next;

    public IHandler SetNext(IHandler next)
    {
        _next = next;
        return next;
    }

    public virtual string Handle(Request request)
    {
        return _next?.Handle(request) ?? "No handler could approve the request.";
    }
}