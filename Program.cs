using Raylib_cs;

namespace Chip8Sharp;

public static class Program
{
    public static void Main() 
    {
        Console.Write("Enter filename: ");

        var fileName = Console.ReadLine();

        var chip8 = new Chip8();

        if (chip8.TryLoadRom(fileName))
        {
            Start(chip8);
        }
    }

    public static void Start(Chip8 chip8)
    {
        while (!Raylib.WindowShouldClose())
        {
            chip8.FetchInstruction();
            chip8.DecodeInstruction();
        }
    }
}