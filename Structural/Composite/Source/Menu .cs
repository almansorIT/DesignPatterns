//composite
public class Menu : IMenuComponent
{
    private readonly List<IMenuComponent> _menuComponents = new List<IMenuComponent>();

    public void Add(IMenuComponent menuComponent)
    {
        _menuComponents.Add(menuComponent);
    }

    public void Remove(IMenuComponent menuComponent)
    {
        _menuComponents.Remove(menuComponent);
    }

    public void Display()
    {
        foreach (var menuComponent in _menuComponents)
        {
            menuComponent.Display();
        }
    }
}