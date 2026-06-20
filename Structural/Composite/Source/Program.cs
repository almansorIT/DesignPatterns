public class Program
{
    public static void Main()
    {
        var mainMenu = new Menu();
        var breakfastMenu = new Menu();
        var lunchMenu = new Menu();

        breakfastMenu.Add(new MenuItem("Pancakes", 5.99m));
        breakfastMenu.Add(new MenuItem("Waffles", 6.99m));

        lunchMenu.Add(new MenuItem("Burger", 8.99m));
        lunchMenu.Add(new MenuItem("Salad", 7.99m));

        mainMenu.Add(breakfastMenu);
        mainMenu.Add(lunchMenu);

        mainMenu.Display();
    }
}