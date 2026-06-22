using Microsoft.Extensions.DependencyInjection;
using OpaqueFacadeSubSystem.Abstractions;
using TransparentFacadeSubSystem;
using TransparentFacadeSubSystem.Abstractions;
using Xunit;

// ---------------------------------------------------------------------------
// Opaque façade.
//
// Everything behind the façade (the concrete ECommerceFacade and the three
// subsystems) is internal. The ONLY way a client can reach it is through the
// public interface, resolved from the DI extension — which is exactly the
// guarantee the opaque variant is meant to provide. So the tests go through DI.
// ---------------------------------------------------------------------------
public class OpaqueFacadeTests
{
    private static IECommerceOpaqueFacade ResolveFacade()
    {
        var provider = new ServiceCollection()
            .AddOpaqueFacadeSubSystem()
            .BuildServiceProvider();

        return provider.GetRequiredService<IECommerceOpaqueFacade>();
    }

    [Fact]
    public void Registration_ExposesOnlyTheFacadeInterface()
    {
        // The client receives a working IECommerceOpaqueFacade...
        var facade = ResolveFacade();

        Assert.NotNull(facade);
        Assert.IsAssignableFrom<IECommerceOpaqueFacade>(facade);
    }

    [Fact]
    public void PlaceOrder_OrchestratesTheHiddenSubsystems()
    {
        var facade = ResolveFacade();

        // One façade call drives stock-check + order-creation + shipping, and
        // the caller only sees the single rolled-up result.
        var result = facade.PlaceOrder("SKU-1", 2);

        Assert.Equal("Order 123 placed successfully.", result);
    }

    [Fact]
    public void CheckOrderStatus_DelegatesToOrderProcessing()
    {
        var facade = ResolveFacade();

        Assert.Equal("Order Shipped", facade.CheckOrderStatus(123));
    }

    [Fact]
    public void Registration_IsSingleton()
    {
        var provider = new ServiceCollection()
            .AddOpaqueFacadeSubSystem()
            .BuildServiceProvider();

        var first = provider.GetRequiredService<IECommerceOpaqueFacade>();
        var second = provider.GetRequiredService<IECommerceOpaqueFacade>();

        Assert.Same(first, second);
    }
}

// ---------------------------------------------------------------------------
// Transparent façade.
//
// Here the subsystems are public interfaces, so the façade can be constructed
// directly with hand-picked (and swappable) implementations. That visibility
// is the whole trade-off of the transparent variant — and the tests exploit it.
// ---------------------------------------------------------------------------
public class TransparentFacadeTests
{
    private static ECommerceFacade BuildFacade(IInventoryService? inventory = null) =>
        new(
            inventory ?? new InventoryService(),
            new OrderProcessingService(),
            new ShippingService());

    // An out-of-stock inventory to prove the façade reacts to subsystem state.
    private sealed class OutOfStockInventory : IInventoryService
    {
        public bool CheckStock(string productId, int quantity) => false;
    }

    [Fact]
    public void Facade_ImplementsTheTargetInterface()
    {
        Assert.IsAssignableFrom<IECommerceTransparentFacade>(BuildFacade());
    }

    [Fact]
    public void PlaceOrder_WhenStockAvailable_PlacesTheOrder()
    {
        var facade = BuildFacade();

        Assert.Equal("Order 123 placed successfully.", facade.PlaceOrder("SKU-1", 2));
    }

    [Fact]
    public void PlaceOrder_WhenOutOfStock_FailsWithoutCreatingAnOrder()
    {
        // Because the subsystem is an injectable interface, we can swap in a
        // failing inventory and watch the façade short-circuit.
        var facade = BuildFacade(new OutOfStockInventory());

        Assert.Equal("Order failed due to insufficient stock.", facade.PlaceOrder("SKU-1", 2));
    }

    [Fact]
    public void CheckOrderStatus_DelegatesToOrderProcessing()
    {
        Assert.Equal("Order Shipped", BuildFacade().CheckOrderStatus(123));
    }

    [Theory]
    [InlineData("inventoryService")]
    [InlineData("orderProcessingService")]
    [InlineData("shippingService")]
    public void Constructor_NullSubsystem_Throws(string nullArg)
    {
        IInventoryService? inventory = nullArg == "inventoryService" ? null : new InventoryService();
        IOrderProcessingService? orders = nullArg == "orderProcessingService" ? null : new OrderProcessingService();
        IShippingService? shipping = nullArg == "shippingService" ? null : new ShippingService();

        var ex = Assert.Throws<ArgumentNullException>(
            () => new ECommerceFacade(inventory!, orders!, shipping!));

        Assert.Equal(nullArg, ex.ParamName);
    }

    [Fact]
    public void Registration_ResolvesFacadeAndSubsystems()
    {
        var provider = new ServiceCollection()
            .AddTransparentFacadeSubSystem()
            .BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IECommerceTransparentFacade>());
        Assert.NotNull(provider.GetRequiredService<IInventoryService>());
        Assert.NotNull(provider.GetRequiredService<IOrderProcessingService>());
        Assert.NotNull(provider.GetRequiredService<IShippingService>());
    }
}
