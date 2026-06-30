namespace ImprovedChainOfResponsibility;

// Concrete handlers for the "Improved" iteration. Because the base class
// supplies CanHandle, each handler only declares the name it owns and what
// it does. The trade-off (and the reason the "Final" version exists): a
// handler that needs to match *several* names can't, without re-overriding
// CanHandle - which is what the Single/Multiple split in the Final version
// solves.
public class AlarmTriggeredHandler : MessageHandlerBase
{
    public AlarmTriggeredHandler(IMessageHandler? next = null)
        : base(next) { }

    protected override string HandledMessageName => "AlarmTriggered";

    protected override void Process(Message message)
    {
        Console.WriteLine($"Alarm triggered! Payload: {message.Payload}");
    }
}

public class AlarmPausedHandler : MessageHandlerBase
{
    public AlarmPausedHandler(IMessageHandler? next = null)
        : base(next) { }

    protected override string HandledMessageName => "AlarmPaused";

    protected override void Process(Message message)
    {
        Console.WriteLine($"Alarm paused. Payload: {message.Payload}");
    }
}

public class AlarmStoppedHandler : MessageHandlerBase
{
    public AlarmStoppedHandler(IMessageHandler? next = null)
        : base(next) { }

    protected override string HandledMessageName => "AlarmStopped";

    protected override void Process(Message message)
    {
        Console.WriteLine($"Alarm stopped. Payload: {message.Payload}");
    }
}
