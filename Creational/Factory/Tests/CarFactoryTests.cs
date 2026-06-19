using Xunit;

public class CarFactoryTests
{
    [Fact]
    public void CreateCar_WithSedan_ReturnsSedanInstance()
    {
        var factory = new carFactory();

        ICar car = factory.CreateCar("Sedan");

        Assert.IsType<Sedan>(car);
    }

    [Fact]
    public void CreateCar_WithSUV_ReturnsSuvInstance()
    {
        var factory = new carFactory();

        ICar car = factory.CreateCar("SUV");

        Assert.IsType<SUV>(car);
    }

    [Fact]
    public void CreateCar_ReturnsICarImplementation()
    {
        var factory = new carFactory();

        ICar car = factory.CreateCar("Sedan");

        Assert.IsAssignableFrom<ICar>(car);
    }

    [Theory]
    [InlineData("sedan")]
    [InlineData("suv")]
    [InlineData("Truck")]
    [InlineData("")]
    [InlineData("  ")]
    public void CreateCar_WithUnknownType_ThrowsArgumentException(string carType)
    {
        var factory = new carFactory();

        var ex = Assert.Throws<ArgumentException>(() => factory.CreateCar(carType));
        Assert.Equal("Invalid car type", ex.Message);
    }

    [Theory]
    [InlineData("Sedan", "Your car is a sedan.")]
    [InlineData("SUV", "Your car is an SUV.")]
    public void Drive_WritesExpectedMessageToConsole(string carType, string expected)
    {
        var factory = new carFactory();
        ICar car = factory.CreateCar(carType);

        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            car.Drive();
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        Assert.Equal(expected, writer.ToString().Trim());
    }
}
