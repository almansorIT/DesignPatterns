namespace Creational.Factory;
class Program
{
public static void Main(string[] args)
{
    var carFactory = new carFactory();
    var car = carFactory.CreateCar("SUV");
    car.Drive();    


}
}