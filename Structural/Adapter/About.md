# Adapter Pattern

A **structural** design pattern that lets two incompatible interfaces work together. It wraps an
existing class (the **adaptee**) in a new class (the **adapter**) that exposes the interface the
client actually expects (the **target**) — without touching the adaptee's own code.

## The problem

Your application is built around a clean abstraction it controls — here, `IGreeter` with a single
`Greet()` method. Then you need to pull in a third-party (or legacy) component that does the same
job but speaks a different "shape": `ExternalGreeter` only offers `GetGreeting(string name)`.

You can't change `ExternalGreeter` — it's external. The naive fix is to teach every caller about
both shapes:

```csharp
// The client now has to know which greeter it's holding and how each one is called.
if (greeter is ExternalGreeter ext)
    Console.WriteLine(ext.GetGreeting("Almansoor"));
else
    Console.WriteLine(greeter.Greet());
```

This leaks the foreign interface into your whole codebase and has to be repeated everywhere a
greeting is produced.

## How it solves it

The pattern introduces an adapter that **implements the target interface** and, internally,
**delegates** to the adaptee — translating the call from the shape the client wants into the shape
the adaptee provides.

- [`IGreeter`](Source/Feature/IGreeter.cs) — the **target**. The interface the client depends on
  (`Greet()`).
- [`ExternalGreeter`](Source/Feature/ExternalGreeter.cs) — the **adaptee**. Useful, but its
  `GetGreeting(string name)` method doesn't match what the client expects. Left completely
  untouched.
- [`ExternalGreeterAdapter`](Source/Feature/ExternalGreeterAdapter.cs) — the **adapter**. It
  implements `IGreeter`, holds an `ExternalGreeter`, and maps `Greet()` onto `GetGreeting(...)`.
- [`Program`](Source/Program.cs) — the **client**. It only ever sees `IGreeter`; the DI container
  hands it the adapter:

```csharp
builder.Services.AddSingleton<ExternalGreeter>();
builder.Services.AddSingleton<IGreeter, ExternalGreeterAdapter>();

app.MapGet("/", (IGreeter greeter) => greeter.Greet());
```

The endpoint asks for an `IGreeter` and is none the wiser that the work is really being done by an
`ExternalGreeter` underneath. Swapping the adaptee for a different one later means changing one
registration line — not the endpoint.

> This is the **object adapter** variant (composition: the adapter *holds* an instance of the
> adaptee). The alternative is the **class adapter** variant, which inherits from the adaptee —
> not possible in C# when you'd need to adapt more than one class, since C# has no multiple
> inheritance. Object adapter is the more flexible default.

> Note: the current `Greet()` hardcodes the name `"Almansoor"` for demonstration. In production
> the name would come from the request or the adapter's constructor, so the adapter stays
> reusable rather than tied to one value.

## From a SOLID point of view

- **S — Single Responsibility:** the adapter has exactly one job — translate between `IGreeter`
  and `ExternalGreeter`. The adaptee keeps its own responsibility; the client keeps its own.
- **O — Open/Closed:** you integrate a foreign type by *adding* an adapter, never by editing the
  client or the adaptee. New adaptees get new adapters.
- **L — Liskov Substitution:** `ExternalGreeterAdapter` is a fully valid `IGreeter` — anywhere the
  app expects an `IGreeter`, the adapter substitutes cleanly and honours the contract.
- **I — Interface Segregation:** the client depends only on the slim `IGreeter`, not on the wider
  surface of `ExternalGreeter`. The foreign API never leaks past the adapter.
- **D — Dependency Inversion:** the client depends on the `IGreeter` abstraction, and the concrete
  adapter is wired in via DI. High-level code never references the low-level external type.

## Tests

[`Tests/AdapterTests.cs`](Tests/AdapterTests.cs) covers the behaviour that makes the pattern work:

- the adapter is assignable to the target interface (`IGreeter`),
- `Greet()` delegates to the adaptee and surfaces its output,
- the adapter changes shape but not behaviour (same text as calling the adaptee directly),
- a null adaptee is rejected in the constructor,
- repeated calls are deterministic.

Run them with:

```bash
dotnet test Structural/Adapter/Tests/Tests.csproj
```
