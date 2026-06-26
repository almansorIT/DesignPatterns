public class ManagerHandler : Handler
{
    public override string Handle(Request request)
    {
        if (request.Amount <= 1000)
        {
            return "Manager approved the request.";
        }

        return base.Handle(request);
    }
}