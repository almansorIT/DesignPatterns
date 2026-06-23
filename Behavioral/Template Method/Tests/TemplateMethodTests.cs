using TemplateMethod;
using Xunit;

public class TemplateMethodTests
{
    // A test-only subclass that records whether the primitive step ran.
    // Used to prove the template method (IndexOf) controls the workflow.
    private sealed class SpySearchMachine : SearchMachine
    {
        public int FindCallCount { get; private set; }

        public SpySearchMachine(params int[] values) : base(values) { }

        protected override int? Find(int value)
        {
            FindCallCount++;
            return null;
        }
    }

    // --- The template method skeleton (defined once in the base) ---

    [Fact]
    public void IndexOf_EmptyInput_ReturnsNull_WithoutCallingFind()
    {
        var spy = new SpySearchMachine();

        var result = spy.IndexOf(42);

        Assert.Null(result);
        Assert.Equal(0, spy.FindCallCount); // the guard short-circuits the primitive
    }

    [Fact]
    public void IndexOf_NonEmptyInput_DelegatesToFind()
    {
        var spy = new SpySearchMachine(1, 2, 3);

        spy.IndexOf(99);

        Assert.Equal(1, spy.FindCallCount); // the skeleton calls down into the subclass
    }

    [Fact]
    public void Constructor_NullValues_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SpySearchMachine(null!));
    }

    // --- LinearSearchMachine (one concrete primitive) ---

    [Fact]
    public void Linear_FindsValue_AtInsertionOrderIndex()
    {
        var machine = new LinearSearchMachine(1, 10, 5, 2, 123, 333, 4);

        Assert.Equal(2, machine.IndexOf(5));   // 5 sits at index 2 as inserted
        Assert.Equal(0, machine.IndexOf(1));
        Assert.Equal(6, machine.IndexOf(4));
    }

    [Fact]
    public void Linear_MissingValue_ReturnsNull()
    {
        var machine = new LinearSearchMachine(1, 10, 5, 2);

        Assert.Null(machine.IndexOf(7));
    }

    // --- BinarySearchMachine (another concrete primitive) ---

    [Fact]
    public void Binary_FindsValue_RegardlessOfInsertionOrder()
    {
        // Constructor sorts, so an unsorted argument list still works.
        var machine = new BinarySearchMachine(10, 1, 5, 2, 4, 3);

        // After sorting -> 1,2,3,4,5,10. Value 5 is at sorted index 4.
        Assert.Equal(4, machine.IndexOf(5));
        Assert.Equal(0, machine.IndexOf(1));
        Assert.Equal(5, machine.IndexOf(10));
    }

    [Fact]
    public void Binary_MissingValue_ReturnsNull()
    {
        var machine = new BinarySearchMachine(1, 2, 3, 4, 5);

        Assert.Null(machine.IndexOf(99));
    }

    // --- Liskov substitutability: callers depend only on the abstraction ---

    [Theory]
    [InlineData(typeof(LinearSearchMachine))]
    [InlineData(typeof(BinarySearchMachine))]
    public void AnyMachine_IsUsableThroughTheBaseAbstraction(Type machineType)
    {
        SearchMachine machine = machineType == typeof(LinearSearchMachine)
            ? new LinearSearchMachine(3, 1, 2)
            : new BinarySearchMachine(3, 1, 2);

        // Same contract honoured by every subclass: hit -> index, miss -> null.
        Assert.NotNull(machine.IndexOf(2));
        Assert.Null(machine.IndexOf(99));
    }
}
