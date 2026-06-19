namespace Structural.Decorator;
public class Program
{
    public static void Main(string[] args)
    {
        IMessageSender messageSender = new LoggingDecorator(new EncryptionDecorator(new MessageSender()));
        messageSender.Send("Hello, World!");
    }
}