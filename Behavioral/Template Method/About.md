# Template Method Pattern

The Template Method defines the **skeleton of an algorithm** in a base class and
lets subclasses override specific steps without changing the algorithm's overall
structure. The "template" is a method that calls a fixed sequence of steps; some
of those steps are implemented once in the base, others are left abstract for
subclasses to fill in.

> Behavioral pattern · "Don't call us, we'll call you" (Hollywood Principle).

## The problem

Several classes share the **same high-level workflow** but differ in one or two
steps. Searching a collection always means: guard against an empty input, then
locate the value. *Where* the value is located differs — a linear scan versus a
binary search — but the surrounding workflow (the empty guard, returning a
nullable index) is identical.

Without the pattern, every search class re-implements the whole flow and the
shared parts get **copy-pasted** into each one. The empty-array check, the
return convention, the validation — all duplicated, and all free to drift out of
sync the moment someone edits one copy and forgets the others.

## How it solves it

`SearchMachine` is the **abstract base** that owns the algorithm:

```csharp
public int? IndexOf(int value)          // the Template Method (the skeleton)
{
    if (Values.Length == 0) { return null; }   // invariant step, fixed for all
    return Find(value);                          // the varying step, deferred
}

protected abstract int? Find(int value);         // primitive operation / hook
```

- `IndexOf` is the **template method** — `public`, non-virtual, and it controls
  the sequence. Callers only ever touch this. The shape of the algorithm can no
  longer drift between implementations because there is only one copy of it.
- `Find` is the **primitive operation** — `protected abstract`, the single step
  the base intentionally leaves open.
- `LinearSearchMachine` and `BinarySearchMachine` override **only** `Find`.
  `BinarySearchMachine` additionally pushes its precondition (a sorted array)
  into its constructor, keeping its `Find` honest.

This is the **inversion of control** at the heart of the pattern: the base class
calls down into the subclass ("don't call us, we'll call you"), not the other
way around. Subclasses never re-implement the workflow; they plug into it.

## From a SOLID POV

- **SRP** — the base class is responsible for the *workflow*; each subclass is
  responsible for *one search strategy*. Two reasons to change, two homes.
- **OCP** — adding a new search (e.g. interpolation search) means a new subclass
  overriding `Find`. The base `SearchMachine` and its template method are never
  touched. Open for extension, closed for modification.
- **LSP** — every subclass honours the `int? IndexOf(int)` contract: a hit
  returns the index, a miss returns `null`. Any `SearchMachine` is freely
  substitutable, which is exactly what lets `Program.cs` resolve and iterate
  `IEnumerable<SearchMachine>` without caring which concrete type it has.
- **DIP** — `Program.cs` depends on the abstraction `SearchMachine`, registered
  in DI, not on the concrete machines.

## Template Method vs. Strategy

Both let an algorithm step vary. The difference is the mechanism:

- **Template Method** varies via **inheritance** — the variation is chosen at
  compile time by subclassing, and the base owns the skeleton.
- **Strategy** varies via **composition** — the variation is an object injected
  at runtime, and the context delegates to it.

Reach for Template Method when the *workflow* is fixed and only steps differ;
reach for Strategy when you want to swap the whole behaviour at runtime.

## Try it

```http
GET http://localhost:5000/search/6
```

Returns one `SearchResult` per registered machine, each reporting whether it
found the number and at what index.
