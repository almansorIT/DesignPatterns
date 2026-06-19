using Structural.Decorator;
public abstract class MessageSenderDecorator : IMessageSender
{
    protected readonly IMessageSender _messageSender;

    protected MessageSenderDecorator(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    public virtual void Send(string message)
    {
        _messageSender.Send(message);
    }
}