using Xunit;

public class AdapterTests
{
    [Fact]
    public void Adapter_ImplementsTargetInterface()
    {
        var adapter = new ExternalGreeterAdapter(new ExternalGreeter());

        // The whole point of the pattern: the adaptee can stand in wherever the
        // client expects the target abstraction.
        Assert.IsAssignableFrom<IGreeter>(adapter);
    }

    [Fact]
    public void Greet_DelegatesToTheAdaptee()
    {
        IGreeter greeter = new ExternalGreeterAdapter(new ExternalGreeter());

        var result = greeter.Greet();

        // The adapter translates the target's parameterless Greet() into the
        // adaptee's GetGreeting(name) call, so its output must come through.
        Assert.Equal("Hello, Almansoor from the external greeter!", result);
    }

    [Fact]
    public void Greet_ProducesSameTextAsCallingTheAdapteeDirectly()
    {
        var adaptee = new ExternalGreeter();
        IGreeter greeter = new ExternalGreeterAdapter(adaptee);

        // The adapter must not alter the adaptee's behaviour, only its shape.
        Assert.Equal(adaptee.GetGreeting("Almansoor"), greeter.Greet());
    }

    [Fact]
    public void Constructor_NullAdaptee_Throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => new ExternalGreeterAdapter(null!));

        Assert.Equal("externalGreeter", ex.ParamName);
    }

    [Fact]
    public void Greet_IsDeterministic_AcrossMultipleCalls()
    {
        IGreeter greeter = new ExternalGreeterAdapter(new ExternalGreeter());

        var first = greeter.Greet();
        var second = greeter.Greet();

        Assert.Equal(first, second);
    }
}
