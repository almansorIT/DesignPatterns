using Creational.Factory;
public class carFactory
{
    public ICar CreateCar(string carType)
    {
        switch (carType)
        {
            case "Sedan":
                return new Sedan();
            case "SUV":
                return new SUV();
            default:
                throw new ArgumentException("Invalid car type");
        }
    }
}