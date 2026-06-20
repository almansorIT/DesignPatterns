using Xunit;

public class CompositeTests
{
    private static string CaptureDisplay(IMenuComponent component)
    {
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            component.Display();
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return writer.ToString();
    }

    [Fact]
    public void MenuItem_StoresNameAndPrice()
    {
        var item = new MenuItem("Pancakes", 5.99m);

        Assert.Equal("Pancakes", item.Name);
        Assert.Equal(5.99m, item.Price);
    }

    [Fact]
    public void MenuItem_Display_WritesNameAndPrice()
    {
        var item = new MenuItem("Pancakes", 5.99m);

        var output = CaptureDisplay(item).Trim();

        Assert.Equal($"{item.Name} - ${item.Price}", output);
    }

    [Fact]
    public void Menu_IsTreatedAsAComponent()
    {
        var menu = new Menu();

        Assert.IsAssignableFrom<IMenuComponent>(menu);
    }

    [Fact]
    public void MenuItem_IsTreatedAsAComponent()
    {
        var item = new MenuItem("Coffee", 2.50m);

        Assert.IsAssignableFrom<IMenuComponent>(item);
    }

    [Fact]
    public void Menu_Display_RendersEachChildInOrder()
    {
        var menu = new Menu();
        menu.Add(new MenuItem("Burger", 8.99m));
        menu.Add(new MenuItem("Salad", 7.99m));

        var lines = CaptureDisplay(menu)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length);
        Assert.Equal("Burger - $8.99", lines[0]);
        Assert.Equal("Salad - $7.99", lines[1]);
    }

    [Fact]
    public void Menu_Display_WithNoChildren_WritesNothing()
    {
        var menu = new Menu();

        var output = CaptureDisplay(menu);

        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void Menu_Remove_ExcludesComponentFromDisplay()
    {
        var menu = new Menu();
        var pancakes = new MenuItem("Pancakes", 5.99m);
        var waffles = new MenuItem("Waffles", 6.99m);
        menu.Add(pancakes);
        menu.Add(waffles);

        menu.Remove(pancakes);

        var lines = CaptureDisplay(menu)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Single(lines);
        Assert.Equal("Waffles - $6.99", lines[0]);
    }

    [Fact]
    public void Menu_Display_RecursesIntoNestedMenus()
    {
        var breakfastMenu = new Menu();
        breakfastMenu.Add(new MenuItem("Pancakes", 5.99m));
        breakfastMenu.Add(new MenuItem("Waffles", 6.99m));

        var lunchMenu = new Menu();
        lunchMenu.Add(new MenuItem("Burger", 8.99m));

        var mainMenu = new Menu();
        mainMenu.Add(breakfastMenu);
        mainMenu.Add(lunchMenu);

        var lines = CaptureDisplay(mainMenu)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(
            new[] { "Pancakes - $5.99", "Waffles - $6.99", "Burger - $8.99" },
            lines);
    }
}
