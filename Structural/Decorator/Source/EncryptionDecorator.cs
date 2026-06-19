using Structural.Decorator;
public class EncryptionDecorator : MessageSenderDecorator
{
    public EncryptionDecorator(IMessageSender messageSender) : base(messageSender) { }

    public override void Send(string message)
    {
        var encrypted = $"[Encrypted]{message}";
        Console.WriteLine("Encrypting message...");
        base.Send(encrypted);
    }
}