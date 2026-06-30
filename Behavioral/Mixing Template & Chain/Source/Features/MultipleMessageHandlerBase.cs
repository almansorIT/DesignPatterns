namespace FinalChainOfResponsibility;

// Same idea as SingleMessageHandlerBase, but matches any of several names.
public abstract class MultipleMessageHandlerBase : MessageHandlerBase
{
    protected MultipleMessageHandlerBase(IMessageHandler? next = null)
        : base(next) { }

    protected override bool CanHandle(Message message)
    {
        return HandledMessagesName.Contains(message.Name);
    }

    protected abstract string[] HandledMessagesName { get; }
}
