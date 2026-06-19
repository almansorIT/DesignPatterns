# Design Patterns in C# / .NET

A hands-on catalogue of classic **Gang of Four (GoF)** design patterns, implemented as small, focused console projects in modern C# (`.NET 10`, nullable + implicit usings enabled).

The goal of this repository is to demonstrate not just *what* each pattern is, but **when** and **why** I would reach for it in production code — with examples drawn from the kind of problems that come up in real ASP.NET Core applications.

Patterns are organised by the three GoF families:

| Category | Concern | Patterns in this repo |
|----------|---------|-----------------------|
| **Creational** | How objects are created | [Factory](#factory) · [Singleton](#singleton) |
| **Structural** | How objects are composed | [Composite](#composite) · [Decorator](#decorator) |
| **Behavioral** | How objects communicate | [Strategy](#strategy) |

---

## Repository structure

```
DesignPatterns/
├── Creational/
│   ├── Factory/Source/        # Encapsulate object construction
│   └── Singleton/Source/      # One shared instance, controlled access
├── Structural/
│   ├── Composite/Source/      # Treat trees and leaves uniformly
│   └── Decorator/Source/      # Add behaviour without subclassing
├── Behavioral/
│   └── Strategy/Source/       # Swap algorithms at runtime
└── DesignPatterns.sln
```

Each pattern lives in its own self-contained console project so it can be run and studied in isolation.

---

## Creational Patterns

### Factory

**Intent:** Encapsulate the decision of *which* concrete type to instantiate, so calling code depends on an abstraction instead of a `new` expression.

**Problem it solves:** Scattering `new ConcreteType()` across a codebase couples callers to specific implementations and makes them painful to change or test. A factory centralises that decision behind a single seam.

**Where it shows up in .NET:** `ILoggerFactory`, `IHttpClientFactory`, `DbProviderFactory`, and the `IServiceProvider` itself are all factories. Whenever a request shape determines which handler or strategy to build, a factory keeps that branching in one place.

📂 [`Creational/Factory/Source`](Creational/Factory/Source)

### Singleton

**Intent:** Guarantee a class has exactly one instance and provide a single global point of access to it.

**Problem it solves:** Some resources are genuinely shared and expensive to recreate — a configuration cache, a connection pool, an in-memory metrics registry. A singleton ensures one consistent instance and prevents accidental duplication.

**Where it shows up in .NET:** This is the spirit of `services.AddSingleton<T>()`. In production I prefer **container-managed singletons** over the hand-rolled static variant, because the DI container handles thread-safety and lifetime while keeping the type testable. The classic implementation here documents *why* that trade-off exists.

📂 [`Creational/Singleton/Source`](Creational/Singleton/Source)

---

## Structural Patterns

### Composite

**Intent:** Compose objects into tree structures and let clients treat individual objects and compositions uniformly through a shared interface.

**Problem it solves:** When part-whole hierarchies exist (a folder containing files *and* folders, a UI made of panels containing controls), client code shouldn't have to constantly ask "is this a leaf or a branch?". Composite removes that distinction.

**Where it shows up in .NET:** The Razor/Blazor render tree, expression trees, and validation rule groups all use this shape — a node and a collection of nodes implement the same contract.

📂 [`Structural/Composite/Source`](Structural/Composite/Source)

### Decorator

**Intent:** Attach additional responsibilities to an object dynamically by wrapping it, providing a flexible alternative to subclassing for extending behaviour.

**Problem it solves:** Adding caching, logging, retries, or compression by creating subclasses leads to a combinatorial explosion. Decorators let you layer those concerns at runtime, one wrapper at a time, each honouring the same interface.

**Where it shows up in .NET:** ASP.NET Core **middleware** is the decorator pattern over the request pipeline. `Stream` wrappers (`GZipStream`, `BufferedStream`, `CryptoStream`) and DI decorator registrations (e.g. wrapping a repository with a caching layer) are everyday examples.

📂 [`Structural/Decorator/Source`](Structural/Decorator/Source)

---

## Behavioral Patterns

### Strategy

**Intent:** Define a family of interchangeable algorithms, encapsulate each one, and make them swappable at runtime behind a common interface.

**Problem it solves:** Replaces sprawling `if/else` or `switch` blocks that select behaviour with polymorphism. New behaviour means a new class, not an edit to existing logic — keeping code aligned with the Open/Closed Principle.

**Where it shows up in .NET:** Pricing/discount rules, payment providers, sorting comparers (`IComparer<T>`), and authentication handlers are all strategies. Combined with DI, selecting a strategy by key is a clean, testable alternative to conditionals.

📂 [`Behavioral/Strategy/Source`](Behavioral/Strategy/Source)

---

## Running the samples

Each pattern is an independent console app. From the repository root:

```bash
# Run a specific pattern
dotnet run --project Behavioral/Strategy/Source

# Or build everything via the solution
dotnet build DesignPatterns.sln
```

**Requirements:** [.NET 10 SDK](https://dotnet.microsoft.com/download) or later.

---

## Design principles in play

These patterns are not goals in themselves — they're tools for honouring a handful of principles:

- **Open/Closed** — extend behaviour without modifying existing code (Strategy, Decorator).
- **Single Responsibility** — isolate construction (Factory) and individual behaviours (Strategy, Decorator).
- **Dependency Inversion** — depend on abstractions, which is what makes these patterns composable with .NET's DI container.
- **Favour composition over inheritance** — most clearly in Decorator and Composite.

---

> **Note:** This is a learning and portfolio repository. Where the .NET framework already provides a battle-tested implementation (DI lifetimes, middleware, `IHttpClientFactory`), I'd lean on it in production — these hand-written versions exist to show I understand the machinery underneath.
