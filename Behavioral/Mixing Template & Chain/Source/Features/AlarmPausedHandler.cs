namespace FinalChainOfResponsibility;

// Each concrete handler now contains only what is unique to it:
// the name it owns and what it does with the payload.
public class AlarmPausedHandler : SingleMessageHandlerBase
{
    public AlarmPausedHandler(IMessageHandler? next = null)
        : base(next) { }

    protected override string HandledMessageName => "AlarmPaused";

    protected override void Process(Message message)
    {
        Console.WriteLine($"Alarm paused. Payload: {message.Payload}");
    }
}

public class AlarmStoppedHandler : SingleMessageHandlerBase
{
    public AlarmStoppedHandler(IMessageHandler? next = null)
        : base(next) { }

    protected override string HandledMessageName => "AlarmStopped";

    protected override void Process(Message message)
    {
        Console.WriteLine($"Alarm stopped. Payload: {message.Payload}");
    }
}

public class AlarmTriggeredHandler : SingleMessageHandlerBase
{
    public AlarmTriggeredHandler(IMessageHandler? next = null)
        : base(next) { }

    protected override string HandledMessageName => "AlarmTriggered";

    protected override void Process(Message message)
    {
        Console.WriteLine($"Alarm triggered! Payload: {message.Payload}");
    }
}
