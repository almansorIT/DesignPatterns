# Facade Pattern

A **structural** design pattern that puts a single, simple entry point in front of a complicated
subsystem. The client talks to one **façade** object; the façade orchestrates the tangle of
classes behind it — so the caller never has to know the order of operations, the dependencies
between parts, or even that the parts exist.

## The problem

Placing an order in an e-commerce system isn't one step — it's several, and they have to happen in
the right order: check stock, create the order, schedule shipping. Without a façade, every caller
has to know the whole choreography:

```csharp
// The client is forced to know the subsystem, its parts, and their wiring.
if (inventory.CheckStock(productId, quantity))
{
    var orderId = orders.CreateOrder(productId, quantity);
    shipping.ScheduleShipping(orderId);
    // ...repeat this exact sequence everywhere an order is placed.
}
```

This couples every caller to three subsystems and to the *sequence* between them. Change the flow
(add fraud-check, swap shipping providers) and you edit every call site.

## How it solves it

The pattern introduces a façade that exposes a small, intention-revealing API
(`PlaceOrder`, `CheckOrderStatus`) and, internally, coordinates the subsystems. This sample shows
**both variants** of the pattern side by side — the difference is purely about *visibility*.

### Opaque façade — the subsystem is sealed shut

Everything except the façade interface and the DI registration is `internal`. Outside the assembly,
the subsystems simply don't exist.

- [`IECommerceOpaqueFacade`](Source/Feature/Opaque/IECommerceOpaqueFacade.cs) — the public face:
  `PlaceOrder` / `CheckOrderStatus`. The only type a client can touch.
- [`ECommerceFacade`](Source/Feature/Opaque/ECommerceFacade.cs) — the **façade**. `internal`, with an
  `internal` constructor; orchestrates the three services.
- [`InventoryService`](Source/Feature/Opaque/InventoryService.cs),
  [`OrderProcessingService`](Source/Feature/Opaque/OrderProcessingService.cs),
  [`ShippingService`](Source/Feature/Opaque/ShippingService.cs) — the **subsystems**, all `internal`
  and hand-wired inside the registration.
- [`OpaqueStartupExtensions`](Source/Feature/Opaque/StartupExtensions.cs) — `AddOpaqueFacadeSubSystem()`
  news up the concrete graph and exposes *only* `IECommerceOpaqueFacade`.

```csharp
services.AddSingleton<IECommerceOpaqueFacade>(_ =>
    new ECommerceFacade(new InventoryService(), new OrderProcessingService(), new ShippingService()));
```

The trade-off: maximum encapsulation, but the subsystems can't be replaced or unit-tested in
isolation — you can only reach them through the façade.

### Transparent façade — the subsystem is wired through DI

The same façade, but the subsystems are `public` interfaces registered individually. The façade
depends on the abstractions and the container injects them.

- [`IInventoryService` / `IOrderProcessingService` / `IShippingService`](Source/Feature/Transparent/IInventoryService.cs)
  — the subsystem **abstractions**.
- [`InventoryService` etc. + `ECommerceFacade`](Source/Feature/Transparent/InventoryService.cs) — the
  public implementations and the façade that consumes them via constructor injection.
- [`IECommerceTransparentFacade`](Source/Feature/Transparent/IECommerceTransparentFacade.cs) — the
  façade's public contract.
- [`TransparentStartupExtensions`](Source/Feature/Transparent/StartupExtensions.cs) — registers each
  subsystem and the façade with `TryAdd...`, so a caller can substitute any part first.

```csharp
services.TryAddSingleton<IInventoryService, InventoryService>();
services.TryAddSingleton<IOrderProcessingService, OrderProcessingService>();
services.TryAddSingleton<IShippingService, ShippingService>();
services.TryAddSingleton<IECommerceTransparentFacade, ECommerceFacade>();
```

The trade-off: subsystems are swappable and independently testable, at the cost of leaking their
types into the public surface.

### The client

[`Program`](Source/Program.cs) maps endpoints that ask only for a façade — `IECommerceOpaqueFacade`
or `IECommerceTransparentFacade` — and never touches a subsystem directly:

```csharp
app.MapPost("/opaque/PlaceOrder",
    (PlaceOrder order, IECommerceOpaqueFacade facade)
        => facade.PlaceOrder(order.ProductId, order.Quantity));
```

> **Façade vs. Adapter:** the [Adapter](../Adapter/About.md) reshapes *one* interface into another the
> client expects. The Façade isn't about matching a shape — it *simplifies* by hiding *many* objects
> behind one convenient entry point.

## From a SOLID point of view

- **S — Single Responsibility:** the façade has one job — coordinate the order workflow. Each
  subsystem keeps its own single responsibility (stock, orders, shipping).
- **O — Open/Closed:** the workflow can grow behind the façade without touching callers; in the
  transparent variant a subsystem is replaced by registering a different implementation.
- **L — Liskov Substitution:** each façade is a faithful implementation of its interface and
  substitutes anywhere that interface is expected; subsystem swaps honour their contracts.
- **I — Interface Segregation:** clients depend on the slim two-method façade, not on the full
  surface of three subsystems.
- **D — Dependency Inversion:** the client depends on the façade abstraction; the transparent
  façade further depends on subsystem *abstractions* wired in by the container, not on concretes.

## Tests

[`Tests/FacadeTests.cs`](Tests/FacadeTests.cs) covers both variants:

**Opaque** (driven through DI, since the internals are unreachable any other way):
- the registration exposes only `IECommerceOpaqueFacade`,
- `PlaceOrder` orchestrates the hidden subsystems into one rolled-up result,
- `CheckOrderStatus` delegates to order processing,
- the façade is registered as a singleton.

**Transparent** (constructed directly with swappable subsystems):
- the façade is assignable to its target interface,
- `PlaceOrder` succeeds when stock is available,
- swapping in an out-of-stock inventory makes it short-circuit — proving subsystem substitutability,
- `CheckOrderStatus` delegates correctly,
- each null subsystem is rejected in the constructor,
- the registration resolves the façade and every subsystem.

Run them with:

```bash
dotnet test Structural/Facade/Tests/Tests.csproj
```
