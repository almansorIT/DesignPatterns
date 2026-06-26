# Chain of Responsibility Pattern

A **behavioral** design pattern that lets a request travel along a chain of handlers until one of
them deals with it. Each handler decides for itself: *handle the request, or pass it to the next
link.* The sender never knows — or cares — which link does the work; it just drops the request at
the head of the chain.

## The problem

Approving a spending request depends on the amount: a Manager can sign off small sums, a Director
larger ones, and only the CEO can approve the big ones. Without the pattern, that policy collapses
into one branching blob that every caller has to host:

```csharp
// One method owns every threshold, every approver, and the order between them.
if (request.Amount <= 1000)
    return "Manager approved the request.";
else if (request.Amount <= 5000)
    return "Director approved the request.";
else
    return "CEO approved the request.";
```

Every new approval level means editing this `if/else`, and the conditions, the thresholds, and the
ordering are all fused together. The caller is coupled to the *whole* approval hierarchy.

## How it solves it

The pattern turns each approver into its own **handler** that knows only two things: whether *it* can
approve the request, and who comes *next*. The handlers are linked into a chain, and the request is
handed to the front.

- [`Request`](Source/Features/Request.cs) — the message that flows down the chain (`UserRole`,
  `Amount`).
- [`IHandler`](Source/Features/IHandler.cs) — the contract: `SetNext` links the chain (returning the
  *next* handler so calls can fluently cascade), and `Handle` processes the request.
- [`Handler`](Source/Features/Handler.cs) — the abstract base. It holds the `_next` link and provides
  the default `Handle`: forward to the next link, or — if this was the last link — report that
  nobody could approve.

```csharp
public virtual string Handle(Request request)
    => _next?.Handle(request) ?? "No handler could approve the request.";
```

- [`ManagerHandler`](Source/Features/ManagerHandler.cs) approves `<= 1000`, otherwise
  `base.Handle(request)` passes it on.
- [`DirectorHandler`](Source/Features/DirectorHandler.cs) approves `<= 5000`, otherwise passes it on.
- [`CEOHandler`](Source/Features/CEOHandler.cs) is the tail — it approves everything that reaches it.

Each handler owns exactly one threshold and the decision to escalate. Adding a new approval level is
a new class inserted into the chain — no existing handler changes.

### The client

[`Program`](Source/Program.cs) builds the chain once and hands the request to its head. Note the
fluent `SetNext` chaining — each call returns the link it just attached:

```csharp
var manager = new ManagerHandler();
var director = new DirectorHandler();
var ceo = new CEOHandler();

manager.SetNext(director).SetNext(ceo);   // Manager -> Director -> CEO

var result = manager.Handle(request);
```

The client talks only to `manager`. It has no idea how many links follow, what their thresholds are,
or which one ultimately answers.

> **Chain of Responsibility vs. Decorator:** both compose objects into a linked sequence. A
> [Decorator](../../Structural/Decorator/About.md) *always* calls the next wrapper and *adds* to the
> result. A handler in a chain *chooses* whether to pass the request on at all — the first one that
> can handle it stops the chain.

## From a SOLID point of view

- **S — Single Responsibility:** each handler owns one decision (one threshold). The branching policy
  that used to live in one method is split into one class per rule.
- **O — Open/Closed:** new approval levels arrive as new `Handler` subclasses dropped into the chain;
  existing handlers and the client stay untouched.
- **L — Liskov Substitution:** every handler is a `Handler`/`IHandler` and behaves like one — the
  client links and invokes them through the abstraction without special-casing any concrete type.
- **I — Interface Segregation:** `IHandler` exposes just two members — `SetNext` and `Handle` — the
  only things a link in the chain needs.
- **D — Dependency Inversion:** handlers reference the *next* link through `IHandler`, and the client
  drives the chain through that abstraction rather than depending on the concrete approvers.

## Tests

[`Tests/ChainOfResponsibilityTests.cs`](Tests/ChainOfResponsibilityTests.cs) builds the
Manager → Director → CEO chain and verifies the routing and the wiring:

- an amount within the Manager's limit is approved by the Manager and never escalates,
- a mid-range amount falls through to the Director,
- a large amount cascades all the way to the CEO,
- a boundary amount (exactly `1000`, exactly `5000`) is approved by the lower handler — the limits
  are inclusive,
- a lone handler with no successor returns the "no handler could approve" fallback,
- `SetNext` returns the handler just attached, which is what makes the fluent chaining work,
- the chain depends only on `IHandler`, so a custom handler slots in like any built-in one.

Run them with:

```bash
dotnet test "Behavioral/Chain of Responsibility/Tests/Tests.csproj"
```
