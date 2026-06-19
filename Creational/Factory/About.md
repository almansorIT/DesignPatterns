# Factory Pattern

A **creational** design pattern that provides a single place (a "factory") responsible for
creating objects, so calling code asks *what* it wants without knowing *how* the concrete
object is built.

## The problem

Without a factory, the calling code has to `new` up concrete classes itself:

```csharp
ICar car;
if (carType == "Sedan")
    car = new Sedan();
else if (carType == "SUV")
    car = new SUV();
// ...
```

This creation logic gets copy-pasted everywhere a car is needed. Every time you add a new
car type, you have to hunt down and edit every one of those `if`/`switch` blocks. The
calling code is also tightly coupled to the concrete types (`Sedan`, `SUV`), so it can't be
tested or reused against the abstraction alone.

## How it solves it

The pattern centralizes object construction behind one method that returns the **interface**
type, not the concrete type.

- [`ICar`](Source/ICar.cs) — the product abstraction. Callers only ever depend on this.
- [`Sedan`](Source/Sedan.cs) / [`SUV`](Source/SUV.cs) — concrete products implementing `ICar`.
- [`carFactory`](Source/carFactory.cs) — owns the `switch` that maps a request to a concrete product and returns it as `ICar`.
- [`Program`](Source/Program.cs) — the client. It asks the factory for a car and just calls `Drive()`:

```csharp
var carFactory = new carFactory();
ICar car = carFactory.CreateCar("SUV");
car.Drive();
```

The creation logic now lives in exactly one place. The client never sees `new Sedan()` or
`new SUV()` — it depends only on `ICar`.

## From a SOLID point of view

- **S — Single Responsibility:** the responsibility of *deciding which car to build* is
  pulled out of the client and given to `carFactory`. The client's job is to use the car;
  the factory's job is to create it.
- **O — Open/Closed:** behavior can be extended without modifying the client. Adding a new
  car type means adding a class and one `case` in the factory — the `Program` code stays
  untouched.
- **D — Dependency Inversion:** the client depends on the `ICar` abstraction, not on
  concrete `Sedan`/`SUV` classes. High-level code is decoupled from low-level details.

> Note: the `switch` in `carFactory` is the classic **Simple Factory**. It still violates
> Open/Closed *internally* (the factory itself must be edited for each new type). The full
> **Factory Method** pattern pushes that decision into subclasses to close even the factory
> to modification.
