//leaf 
public class MenuItem : IMenuComponent
{
    public string Name { get; }
    public decimal Price { get; }
     public MenuItem(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
    public void Display(){
        Console.WriteLine($"{Name} - ${Price}");
    }

}