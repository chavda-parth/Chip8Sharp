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
        int windowWidth = Constants.DisplayWidth * Constants.WindowScale;
        int windowHeight = Constants.DisplayHeight * Constants.WindowScale;

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

            chip8.FetchInstruction();
            chip8.DecodeInstruction();

            for (int i = 0; i < chip8.Display.Count; i++)
            {
                if (chip8.Display[i] != 0)
                {
                    int row = 
                        (i / Constants.DisplayWidth) * Constants.WindowScale;
                    int col = 
                        (i % Constants.DisplayWidth) * Constants.WindowScale;

                    Raylib.DrawRectangle(
                        col,
                        row,
                        Constants.WindowScale, 
                        Constants.WindowScale, 
                        Color.White
                    );
                }
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}