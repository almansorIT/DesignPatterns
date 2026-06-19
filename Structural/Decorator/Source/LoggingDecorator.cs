using Structural.Decorator;
public class LoggingDecorator : MessageSenderDecorator
{
    public LoggingDecorator(IMessageSender messageSender) : base(messageSender) { }
    public override void Send(string message)
    {
        Console.WriteLine($"Logging message: {message}");
        base.Send(message);
    }
}