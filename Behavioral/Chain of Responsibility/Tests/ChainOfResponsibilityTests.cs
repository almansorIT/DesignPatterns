using Xunit;

// The Source types (Request, IHandler, Handler, ManagerHandler, DirectorHandler,
// CEOHandler) live in the global namespace, so no using is required to reach them.
public class ChainOfResponsibilityTests
{
    // Builds the canonical chain: Manager -> Director -> CEO, and returns its head.
    private static IHandler BuildChain()
    {
        var manager = new ManagerHandler();
        var director = new DirectorHandler();
        var ceo = new CEOHandler();

        manager.SetNext(director).SetNext(ceo);

        return manager;
    }

    [Fact]
    public void SmallAmount_IsApprovedByManager()
    {
        var chain = BuildChain();

        var result = chain.Handle(new Request { Amount = 800 });

        Assert.Equal("Manager approved the request.", result);
    }

    [Fact]
    public void MidAmount_EscalatesToDirector()
    {
        var chain = BuildChain();

        var result = chain.Handle(new Request { Amount = 3000 });

        Assert.Equal("Director approved the request.", result);
    }

    [Fact]
    public void LargeAmount_CascadesAllTheWayToCeo()
    {
        var chain = BuildChain();

        var result = chain.Handle(new Request { Amount = 9000 });

        Assert.Equal("CEO approved the request.", result);
    }

    [Theory]
    [InlineData(1, "Manager approved the request.")]
    [InlineData(1000, "Manager approved the request.")]      // upper bound is inclusive
    [InlineData(1001, "Director approved the request.")]     // one past Manager's limit
    [InlineData(5000, "Director approved the request.")]     // upper bound is inclusive
    [InlineData(5001, "CEO approved the request.")]          // one past Director's limit
    public void Boundaries_AreApprovedByTheExpectedHandler(int amount, string expected)
    {
        var chain = BuildChain();

        Assert.Equal(expected, chain.Handle(new Request { Amount = amount }));
    }

    [Fact]
    public void LoneHandler_WithNoSuccessor_ReturnsTheFallback()
    {
        // A Manager with no next link cannot approve an over-limit request,
        // and there is nowhere to escalate — so the base fallback is returned.
        var manager = new ManagerHandler();

        var result = manager.Handle(new Request { Amount = 9000 });

        Assert.Equal("No handler could approve the request.", result);
    }

    [Fact]
    public void SetNext_ReturnsTheHandlerJustAttached()
    {
        // This return value is what makes the fluent SetNext(...).SetNext(...) chaining work.
        var manager = new ManagerHandler();
        var director = new DirectorHandler();

        var returned = manager.SetNext(director);

        Assert.Same(director, returned);
    }

    [Fact]
    public void CustomHandler_SlotsIntoTheChainThroughTheAbstraction()
    {
        // The chain only depends on IHandler, so a brand-new handler links in
        // exactly like the built-in ones — open for extension, closed for modification.
        var manager = new ManagerHandler();
        manager.SetNext(new VicePresidentHandler());

        var result = manager.Handle(new Request { Amount = 9000 });

        Assert.Equal("VP approved the request.", result);
    }

    // A test-only handler proving the chain is extensible without touching existing links.
    private sealed class VicePresidentHandler : Handler
    {
        public override string Handle(Request request) => "VP approved the request.";
    }
}
