namespace TransparentFacadeSubSystem.Abstractions;

public interface IInventoryService
{
    bool CheckStock(string productId, int quantity);
}
public interface IOrderProcessingService
{
    int CreateOrder(string productId, int quantity);
    string GetOrderStatus(int orderId);
}
public interface IShippingService
{
    void ScheduleShipping(int orderId);
}