using FinalChainOfResponsibility;
using Xunit;

namespace Tests;

// These tests cover the "Final" iteration that Program.cs actually wires up:
// the Template Method (MessageHandlerBase.Handle) driving a Chain of
// Responsibility, with Single/Multiple template layers deciding CanHandle.
//
// Two styles are used:
//  * Spy handlers (below) observe routing directly - no string matching.
//  * A few end-to-end tests drive the *real* Alarm/Multi handlers and assert
//    on their console output, the only thing those void handlers expose.
public class MixingTemplateChainTests
{
    // ---- Template + Chain mechanics (via spies) --------------------------

    [Fact]
    public void Handle_RunsTheFirstLink_WhenItOwnsTheMessage()
    {
        var second = new SpySingleHandler("Second");
        var first = new SpySingleHandler("First", second);

        first.Handle(new Message("First"));

        Assert.True(first.WasCalled);
        Assert.False(second.WasCalled);
    }

    [Fact]
    public void Handle_WalksDownTheChain_UntilALinkClaimsTheMessage()
    {
        var third = new SpySingleHandler("Third");
        var second = new SpySingleHandler("Second", third);
        var first = new SpySingleHandler("First", second);

        first.Handle(new Message("Third"));

        Assert.False(first.WasCalled);
        Assert.False(second.WasCalled);
        Assert.True(third.WasCalled);
    }

    [Fact]
    public void Handle_StopsAtTheFirstMatch_EvenIfALaterLinkAlsoMatches()
    {
        var second = new SpySingleHandler("Dup");
        var first = new SpySingleHandler("Dup", second);

        first.Handle(new Message("Dup"));

        Assert.True(first.WasCalled);
        Assert.False(second.WasCalled);
    }

    [Fact]
    public void Handle_DoesNothing_WhenNoLinkOwnsTheMessage()
    {
        var second = new SpySingleHandler("Second");
        var first = new SpySingleHandler("First", second);

        // Nothing throws, and no link processes an unknown message.
        first.Handle(new Message("Unknown"));

        Assert.False(first.WasCalled);
        Assert.False(second.WasCalled);
    }

    [Fact]
    public void Handle_PassesTheSameMessage_ThroughToProcess()
    {
        var payload = new object();
        var handler = new SpySingleHandler("Ping");

        var message = new Message("Ping", payload);
        handler.Handle(message);

        Assert.Same(message, handler.Received);
        Assert.Same(payload, handler.Received!.Payload);
    }

    // ---- SingleMessageHandlerBase: CanHandle matches one exact name ------

    [Theory]
    [InlineData("Ping", true)]
    [InlineData("ping", false)]   // matching is case-sensitive / exact
    [InlineData("Pong", false)]
    public void SingleHandler_MatchesOnlyItsExactName(string incoming, bool expectedHandled)
    {
        var handler = new SpySingleHandler("Ping");

        handler.Handle(new Message(incoming));

        Assert.Equal(expectedHandled, handler.WasCalled);
    }

    // ---- MultipleMessageHandlerBase: CanHandle matches a set of names ----

    [Theory]
    [InlineData("Foo", true)]
    [InlineData("Bar", true)]
    [InlineData("Baz", true)]
    [InlineData("Qux", false)]
    public void MultipleHandler_MatchesAnyNameInItsSet(string incoming, bool expectedHandled)
    {
        var handler = new SpyMultiHandler(new[] { "Foo", "Bar", "Baz" });

        handler.Handle(new Message(incoming));

        Assert.Equal(expectedHandled, handler.WasCalled);
    }

    [Fact]
    public void SingleAndMultipleHandlers_Coexist_InOneChain()
    {
        // Proves the two template layers are interchangeable links: a message
        // can skip a single-name link and land on a multi-name one.
        var multi = new SpyMultiHandler(new[] { "Foo", "Bar" });
        var single = new SpySingleHandler("Ping", multi);

        single.Handle(new Message("Bar"));

        Assert.False(single.WasCalled);
        Assert.True(multi.WasCalled);
    }

    // ---- End-to-end: the real handlers Program.cs builds -----------------

    [Theory]
    [InlineData("AlarmTriggered", "Alarm triggered!")]
    [InlineData("AlarmPaused", "Alarm paused.")]
    [InlineData("AlarmStopped", "Alarm stopped.")]
    [InlineData("Foo", "SomeMultiHandler handled 'Foo'")]
    [InlineData("Baz", "SomeMultiHandler handled 'Baz'")]
    public void RealChain_RoutesEachMessage_ToTheExpectedHandler(string name, string expectedOutput)
    {
        var chain = BuildRealChain();

        var output = CaptureConsole(() => chain.Handle(new Message(name)));

        Assert.Contains(expectedOutput, output);
    }

    [Fact]
    public void RealChain_ProducesNoOutput_ForAnUnknownMessage()
    {
        var chain = BuildRealChain();

        var output = CaptureConsole(() => chain.Handle(new Message("Unknown")));

        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void RealChain_ForwardsThePayload_IntoTheHandledMessage()
    {
        var chain = BuildRealChain();

        var output = CaptureConsole(() => chain.Handle(new Message("AlarmPaused", "snooze-5min")));

        Assert.Contains("snooze-5min", output);
    }

    // ---- Helpers ---------------------------------------------------------

    // The same chain Program.cs assembles.
    private static IMessageHandler BuildRealChain()
        => new AlarmTriggeredHandler(
               new AlarmPausedHandler(
                   new AlarmStoppedHandler(
                       new SomeMultiHandler())));

    // Console.WriteLine is the only observable effect of the real handlers,
    // so redirect it for the duration of the call. Tests in a single xUnit
    // class run sequentially, so swapping the process-wide writer is safe.
    private static string CaptureConsole(Action action)
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            action();
        }
        finally
        {
            Console.SetOut(original);
        }

        return writer.ToString();
    }

    // Spy links record whether they processed a message, without printing.
    private sealed class SpySingleHandler : SingleMessageHandlerBase
    {
        private readonly string _name;

        public SpySingleHandler(string name, IMessageHandler? next = null)
            : base(next) => _name = name;

        public bool WasCalled { get; private set; }
        public Message? Received { get; private set; }

        protected override string HandledMessageName => _name;

        protected override void Process(Message message)
        {
            WasCalled = true;
            Received = message;
        }
    }

    private sealed class SpyMultiHandler : MultipleMessageHandlerBase
    {
        private readonly string[] _names;

        public SpyMultiHandler(string[] names, IMessageHandler? next = null)
            : base(next) => _names = names;

        public bool WasCalled { get; private set; }

        protected override string[] HandledMessagesName => _names;

        protected override void Process(Message message) => WasCalled = true;
    }
}
