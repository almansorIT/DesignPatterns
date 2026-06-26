public interface IHandler
{
    IHandler SetNext(IHandler next);
    string Handle(Request request);
}