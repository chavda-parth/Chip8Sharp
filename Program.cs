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

        var lastCycleTime = DateTime.UtcNow;

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            Raylib.DrawFPS(12, 12);

            var currentTime = DateTime.UtcNow;
            float deltaTime = 
                (float) ((currentTime - lastCycleTime).TotalMilliseconds);

            if (deltaTime > Constants.CycleDelay)
            {
                lastCycleTime = currentTime;

                chip8.Cycle();

                for (int i = 0; i < chip8.Display.Length; i++)
                {
                    if (chip8.Display[i])
                    {
                        int xPos = 
                            (i % Constants.DisplayWidth) * Constants.WindowScale;
                        int yPos = 
                            (i / Constants.DisplayWidth) * Constants.WindowScale;

                        Raylib.DrawRectangle(
                            xPos,
                            yPos,
                            Constants.WindowScale, 
                            Constants.WindowScale, 
                            Color.White
                        );
                    }
                }
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}