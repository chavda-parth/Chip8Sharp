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
        int windowWidth = Constants.DisplayWidth * 10;
        int windowHeight = Constants.DisplayHeight * 10;

        Raylib.InitWindow(
            windowWidth,
            windowHeight, 
            "Chip8Sharp"
        );

        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            Raylib.DrawFPS(12, 12);


            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}