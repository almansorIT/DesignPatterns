public class DirectorHandler : Handler
{
    public override string Handle(Request request)
    {
        if (request.Amount <= 5000)
        {
            return "Director approved the request.";
        }

        return base.Handle(request);
    }
}