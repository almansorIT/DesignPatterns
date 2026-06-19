using Structural.Decorator;
public class MessageSender : IMessageSender 
{
    public void Send(string message)
    {
        Console.WriteLine($"Sending message: {message}");
    }
}