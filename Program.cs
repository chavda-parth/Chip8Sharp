namespace Chip8Sharp;

public static class Program
{
    public static void Main() 
    {
        Console.Write("Enter filename: ");

        var fileName = Console.ReadLine();

        var chip8 = new Chip8();

        chip8.LoadRom(fileName);
    }
}