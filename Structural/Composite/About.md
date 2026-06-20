# Composite Pattern

A **structural** design pattern that lets you compose objects into tree structures and then
treat individual objects (leaves) and groups of objects (composites) **uniformly** through a
single shared interface.

## The problem

Imagine a restaurant menu. A menu contains menu *items* (a dish with a name and price), but it
also contains *sub-menus* — a breakfast menu and a lunch menu inside the main menu. The client
that renders the menu shouldn't have to care whether it's holding a single dish or a whole
nested menu. The naive approach forces it to:

```csharp
// The client constantly branches on "is this one thing or many things?"
foreach (var entry in mainMenu)
{
    if (entry is MenuItem item)
        Console.WriteLine($"{item.Name} - ${item.Price}");
    else if (entry is Menu submenu)
        foreach (var child in submenu) /* ...and now recurse by hand... */ ;
}
```

This is brittle. Every place that walks the menu has to know the type of each node, handle the
recursion itself, and be edited whenever the tree can hold a new kind of node.

## How it solves it

The pattern defines one component interface that **both** the leaf and the container implement.
The container holds a list of components and, for each operation, simply delegates to its
children — which may themselves be containers. Recursion falls out naturally and the client
calls a single method on the root.

- [`IMenuComponent`](Source/IMenuComponent.cs) — the component abstraction. Both the leaf and
  the composite implement it, so the client treats them interchangeably.
- [`MenuItem`](Source/MenuItem%20.cs) — the **leaf**. It has a `Name` and `Price` and knows how
  to `Display` just itself.
- [`Menu`](Source/Menu%20.cs) — the **composite**. It holds a `List<IMenuComponent>`, exposes
  `Add`/`Remove`, and implements `Display` by looping over its children and calling their
  `Display` — without knowing or caring whether each child is a leaf or another menu.
- [`Program`](Source/Program.cs) — the client. It builds the tree and calls `Display` once on
  the root:

```csharp
var mainMenu = new Menu();
var breakfastMenu = new Menu();
breakfastMenu.Add(new MenuItem("Pancakes", 5.99m));
breakfastMenu.Add(new MenuItem("Waffles", 6.99m));
mainMenu.Add(breakfastMenu);

mainMenu.Display(); // recurses through the whole tree
```

The call to `mainMenu.Display()` cascades down: the main menu asks each child to display, the
breakfast menu asks each of *its* items to display, and so on to any depth — all through the
same `Display()` call.

> Note: this implementation uses the **safe** Composite variant — `Add`/`Remove` live only on
> `Menu`, not on the shared `IMenuComponent` interface. The trade-off is the
> opposite **transparent** variant, which puts `Add`/`Remove` on the interface so leaves and
> composites are *fully* uniform, at the cost of leaves having to implement (or reject) child
> operations that don't apply to them.

## From a SOLID point of view

- **S — Single Responsibility:** `MenuItem` only describes a single dish; `Menu` only manages a
  collection of components and forwards operations to them. Neither mixes concerns.
- **O — Open/Closed:** new kinds of menu components can be added by writing a new
  `IMenuComponent` implementation — the existing `Menu` traversal keeps working untouched.
- **L — Liskov Substitution:** anywhere an `IMenuComponent` is expected, either a `MenuItem` or
  a `Menu` can stand in, because both honour the same `Display` contract.
- **I — Interface Segregation:** the safe variant keeps `IMenuComponent` minimal (just
  `Display`). The transparent variant — where leaves also expose `Add`/`Remove` they can't
  meaningfully support — is the classic way the Composite pattern risks **violating ISP**.
- **D — Dependency Inversion:** every actor depends solely on the `IMenuComponent` abstraction
  rather than on concrete `Menu` or `MenuItem` types, which inverts the dependency flow and is
  exactly what lets a composite hold other components without knowing their concrete types.
