namespace FinalChainOfResponsibility;

// A handler that owns several message names at once.
public class SomeMultiHandler : MultipleMessageHandlerBase
{
    public SomeMultiHandler(IMessageHandler? next = null)
        : base(next) { }

    protected override string[] HandledMessagesName
        => new[] { "Foo", "Bar", "Baz" };

    protected override void Process(Message message)
    {
        Console.WriteLine($"SomeMultiHandler handled '{message.Name}'. Payload: {message.Payload}");
    }
}
