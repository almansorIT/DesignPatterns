# Mixing Template Method & Chain of Responsibility

Two **behavioral** patterns working in one class hierarchy. The
[Chain of Responsibility](../Chain%20of%20Responsibility/About.md) gives us a pipeline of handlers
where a message travels from link to link until one claims it. The
[Template Method](../Template%20Method/About.md) removes the boilerplate *inside* each link: the
base class owns the fixed "decide, then act or forward" algorithm, and subclasses only fill in the
two steps that actually vary.

> Chain of Responsibility decides **how messages flow between handlers**.
> Template Method decides **how each handler is built**. This folder shows them combined.

## The problem

A plain Chain of Responsibility makes *every* handler re-implement the same plumbing. Each link has
to: check whether it owns the message, run its logic if so, and otherwise forward to the next link —
plus null-check that next link first. Written out per handler, that skeleton is copy-pasted into
every class and is free to drift:

```csharp
public void Handle(Message message)
{
    if (message.Name == "AlarmTriggered")   // the "do I own this?" check
        Process(message);                    // the real work
    else if (_next != null)                  // the forwarding plumbing...
        _next.Handle(message);               // ...duplicated in every handler
}
```

The only genuinely unique parts are *which name(s) the handler owns* and *what it does with the
payload*. Everything else is ceremony repeated N times.

## How it solves it

[`MessageHandlerBase`](Source/Features/IMessageHandler.cs) defines the **template method** once —
the skeleton of a single link's behaviour — and defers the two varying steps to subclasses:

```csharp
public void Handle(Message message)          // the Template Method (fixed skeleton)
{
    if (CanHandle(message))                  // primitive operation #1 (deferred)
    {
        Process(message);                    // primitive operation #2 (deferred)
    }
    else if (HasNext())
    {
        _next.Handle(message);               // Chain of Responsibility: forward
    }
}

[MemberNotNullWhen(true, nameof(_next))]
private bool HasNext() => _next != null;     // null-forgiving _next after this returns true

protected abstract bool CanHandle(Message message);
protected abstract void Process(Message message);
```

`Handle` is non-virtual and the only public member — the shape of the algorithm can no longer drift
between handlers because there is exactly one copy. The `[MemberNotNullWhen(true, nameof(_next))]`
attribute lets the compiler know that when `HasNext()` is `true`, `_next` is non-null, so the
nullable reference type analysis stays happy without a `!` on `_next.Handle`.

- [`Message`](Source/Features/IMessageHandler.cs) — the record that travels down the chain
  (`Name`, optional `Payload`).
- [`IMessageHandler`](Source/Features/IMessageHandler.cs) — the one-method contract (`Handle`) that
  lets links reference *the next link* without knowing its concrete type.

### A second template layer: Single vs. Multiple

`CanHandle` is left abstract on purpose, because *how* a handler recognises its messages varies. Two
intermediate base classes fill that step in — each is itself a tiny template, leaving only the data:

- [`SingleMessageHandlerBase`](Source/Features/SingleMessageHandlerBase.cs) — matches **one** exact
  name. Subclasses supply just `HandledMessageName`.
- [`MultipleMessageHandlerBase`](Source/Features/MultipleMessageHandlerBase.cs) — matches **any of a
  set** of names. Subclasses supply just `HandledMessagesName`.

```csharp
// SingleMessageHandlerBase
protected override bool CanHandle(Message message) => message.Name == HandledMessageName;

// MultipleMessageHandlerBase
protected override bool CanHandle(Message message) => HandledMessagesName.Contains(message.Name);
```

The concrete handlers now contain **only** what is unique to them:

- [`AlarmTriggeredHandler` / `AlarmPausedHandler` / `AlarmStoppedHandler`](Source/Features/AlarmPausedHandler.cs)
  each declare one name and a `Process` body.
- [`SomeMultiHandler`](Source/Features/SomeMultiHandler.cs) declares a `string[]` of names and one
  `Process` body.

### Why two iterations live side by side

The folder keeps the design's evolution visible in two namespaces:

- **`ImprovedChainOfResponsibility`** ([`MessageHandlerBase.cs`](Source/Features/MessageHandlerBase.cs),
  [`AlarmTriggeredHandler.cs`](Source/Features/AlarmTriggeredHandler.cs)) — the base provides a
  *default* `CanHandle` that matches a single name. Clean, but a handler that needs to own several
  names has nowhere to put that logic except by re-overriding `CanHandle` — leaking the plumbing back
  into the concrete class.
- **`FinalChainOfResponsibility`** (everything else) — solves that by splitting the `CanHandle`
  decision into the `Single` / `Multiple` template layers above. Single- and multi-name handlers are
  now *interchangeable links*, and neither ever touches forwarding or matching plumbing.

### The client

[`Program`](Source/Program.cs) assembles the chain once — each handler is constructed with the next
as its successor — and exposes it over a minimal-API endpoint:

```csharp
IMessageHandler chain =
    new AlarmTriggeredHandler(
        new AlarmPausedHandler(
            new AlarmStoppedHandler(
                new SomeMultiHandler())));

app.MapPost("/messages/{name}", (string name, string? payload) =>
{
    chain.Handle(new Message(name, payload));
    return Results.Ok($"Dispatched '{name}' into the chain (see console output).");
});
```

The client hands every message to the head of the chain and never knows which link answers — or
whether any does. See [`Source.http`](Source/Source.http) for ready-to-send requests.

> **A note on unhandled messages:** a name no link owns (e.g. `Unknown`) simply falls off the tail
> and nothing happens. In production you would usually append a terminal "dead-letter" handler that
> logs or records anything that reaches it, so messages never vanish silently.

## From a SOLID point of view

- **S — Single Responsibility:** the base owns the *forward-or-handle workflow*; the `Single`/
  `Multiple` layers own *recognition*; each concrete handler owns *one reaction*. Three distinct
  reasons to change, three distinct homes.
- **O — Open/Closed:** a new message handler is a new subclass dropped into the chain. The base
  classes, the existing handlers, and the client never change.
- **L — Liskov Substitution:** every handler is an `IMessageHandler` and behaves like one, so the
  client links and invokes them all through the abstraction — a single-name link and a multi-name
  link are freely interchangeable in the chain.
- **I — Interface Segregation:** `IMessageHandler` exposes the single method (`Handle`) a link in the
  chain actually needs.
- **D — Dependency Inversion:** each handler holds its successor as an `IMessageHandler`, and the
  client drives the chain through that abstraction rather than the concrete handler types.

## Tests

[`Tests/MixingTemplateChainTests.cs`](Tests/MixingTemplateChainTests.cs) covers both the pattern
mechanics and the real handlers (20 tests):

- **Template + Chain mechanics** (via lightweight *spy* handlers that record whether they ran): the
  first owning link processes and stops; a message walks the chain until claimed; the first match
  wins even when a later link would also match; an unowned message falls off the tail without
  throwing; the exact `Message` instance (and its payload) reaches `Process`.
- **The `Single` template layer** matches only its exact name (case-sensitive).
- **The `Multiple` template layer** matches any name in its set and ignores the rest; single- and
  multi-name links coexist in one chain.
- **The real chain** that `Program.cs` builds routes each Alarm/Multi message to the expected handler
  (asserted on console output), produces nothing for an unknown name, and forwards the payload.

Run them with:

```bash
dotnet test "Behavioral/Mixing Template & Chain/Tests/Tests.csproj"
```
