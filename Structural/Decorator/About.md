# Decorator Pattern

A **structural** design pattern that lets you attach new behavior to an object by wrapping it
in another object that shares the same interface, so responsibilities can be added
*dynamically* at runtime without touching the original class.

## The problem

Imagine a `MessageSender` that just sends a message. Now the requirements grow: messages
should be **logged**, and they should be **encrypted** before sending. The naive approach is
to keep editing the one class or to explode into subclasses for every combination:

```csharp
// Editing the original class — it now does three unrelated jobs:
public void Send(string message)
{
    Console.WriteLine($"Logging message: {message}");
    var encrypted = $"[Encrypted]{message}";
    Console.WriteLine($"Sending message: {encrypted}");
}

// ...or a subclass explosion:
// LoggingMessageSender, EncryptingMessageSender,
// LoggingEncryptingMessageSender, EncryptingLoggingMessageSender, ...
```

Both are bad. The first stuffs every responsibility into one class and forces edits for each
new feature. The second multiplies classes for every *combination* and *order* of features,
and the choice is locked in at compile time.

## How it solves it

The pattern defines a wrapper that implements the same interface as the object it wraps, holds
a reference to it, and adds its own behavior **before/after** delegating to the wrapped object.
Because every decorator is also an `IMessageSender`, decorators can be stacked in any order.

- [`IMessageSender`](Source/IMessageSender.cs) — the component abstraction. Both the real
  object and every decorator implement it, so they're interchangeable.
- [`MessageSender`](Source/MessageSender.cs) — the concrete component that does the real work.
- [`MessageSenderDecorator`](Source/MessageSenderDecorator.cs) — the abstract base decorator.
  It holds an inner `IMessageSender` and forwards `Send` to it by default.
- [`LoggingDecorator`](Source/LoggingDecorator.cs) / [`EncryptionDecorator`](Source/EncryptionDecorator.cs)
  — concrete decorators that add one responsibility each, then call `base.Send(...)`.
- [`Program`](Source/Program.cs) — the client. It composes the stack and just calls `Send`:

```csharp
IMessageSender messageSender =
    new LoggingDecorator(new EncryptionDecorator(new MessageSender()));
messageSender.Send("Hello, World!");
```

The call flows outside-in: `LoggingDecorator` logs, then `EncryptionDecorator` encrypts, then
`MessageSender` sends. Each layer adds exactly one behavior, and you reorder or drop layers by
changing only the composition — never the classes themselves.

## From a SOLID point of view

- **S — Single Responsibility:** each class does one thing. `MessageSender` sends,
  `LoggingDecorator` logs, `EncryptionDecorator` encrypts. No class mixes concerns.
- **O — Open/Closed:** new behavior is added by writing a *new* decorator, not by editing
  `MessageSender` or the existing decorators. The system is open to extension, closed to
  modification.
- **L — Liskov Substitution:** every decorator is a fully valid `IMessageSender`, so a wrapped
  object can stand in anywhere the bare component is expected.
- **D — Dependency Inversion:** decorators depend on the `IMessageSender` abstraction, not on
  `MessageSender` concretely — which is exactly why they can wrap each other freely.

> Note: order matters. `Logging(Encryption(sender))` logs the *plaintext* then encrypts;
> `Encryption(Logging(sender))` would log the *ciphertext*. The decorator pattern makes that
> ordering an explicit composition choice rather than something hard-coded in a class.
