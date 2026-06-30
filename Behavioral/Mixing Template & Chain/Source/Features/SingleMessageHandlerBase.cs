namespace FinalChainOfResponsibility;

// A second template layer: it fills in CanHandle (match a single name) but
// leaves Process open. Concrete single-message handlers only declare which
// name they own.
public abstract class SingleMessageHandlerBase : MessageHandlerBase
{
    protected SingleMessageHandlerBase(IMessageHandler? next = null)
        : base(next) { }

    protected override bool CanHandle(Message message)
    {
        return message.Name == HandledMessageName;
    }

    protected abstract string HandledMessageName { get; }
}
