using System.Diagnostics.CodeAnalysis;

namespace ImprovedChainOfResponsibility;

// An earlier iteration of the same idea, kept for comparison with the
// "Final" version. Here the base class already provides a default CanHandle
// (match a single name), so concrete handlers don't even override it - they
// just declare HandledMessageName and Process.
public record Message(string Name, object? Payload = null);

public interface IMessageHandler
{
    void Handle(Message message);
}

public abstract class MessageHandlerBase : IMessageHandler
{
    private readonly IMessageHandler? _next;

    protected MessageHandlerBase(IMessageHandler? next = null)
    {
        _next = next;
    }

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

    // Default decision lives in the base; subclasses can still override it.
    protected virtual bool CanHandle(Message message)
    {
        return message.Name == HandledMessageName;
    }

    protected abstract string HandledMessageName { get; }
    protected abstract void Process(Message message);
}
