using System.Diagnostics.CodeAnalysis;

namespace FinalChainOfResponsibility;

// The message that travels down the chain.
public record Message(string Name, object? Payload = null);

// Chain of Responsibility contract: every link can Handle a message.
public interface IMessageHandler
{
    void Handle(Message message);
}

// Template Method + Chain of Responsibility.
//
// Handle() is the fixed algorithm (the "template"): decide whether this link
// owns the message and, if not, pass it along. The two decisions it relies on
// (CanHandle / Process) are deferred to subclasses.
public abstract class MessageHandlerBase : IMessageHandler
{
    private readonly IMessageHandler? _next;

    protected MessageHandlerBase(IMessageHandler? next = null)
    {
        _next = next;
    }

    // The template method - the shape of the algorithm never changes.
    public void Handle(Message message)
    {
        if (CanHandle(message))
        {
            Process(message);
        }
        else if (HasNext())
        {
            _next.Handle(message);
        }
    }

    [MemberNotNullWhen(true, nameof(_next))]
    private bool HasNext()
    {
        return _next != null;
    }

    // Primitive operations - the "holes" subclasses fill in.
    protected abstract bool CanHandle(Message message);
    protected abstract void Process(Message message);
}
