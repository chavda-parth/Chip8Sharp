using System.Diagnostics.CodeAnalysis;
using System.Numerics;
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

        Image image = Raylib.GenImageColor(
            Constants.DisplayWidth, 
            Constants.DisplayHeight, 
            Color.White);

        Texture2D texture = Raylib.LoadTextureFromImage(image);

        Raylib.UnloadImage(image);

        while (!Raylib.WindowShouldClose())
        {
            bool updateTexture = false;

            var currentTime = DateTime.UtcNow;
            float deltaTime = 
                (float) ((currentTime - lastCycleTime).TotalMilliseconds);

            if (deltaTime > Constants.CycleDelay)
            {
                lastCycleTime = currentTime;

                chip8.Cycle();

                updateTexture = true;
            }


            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            Raylib.DrawFPS(12, 12);

            // for (int i = 0; i < chip8.Display.Count; i++)
            // {
            //     if (chip8.Display[i] != 0)
            //     {
            //         int xPos = 
            //             (i % Constants.DisplayWidth) * Constants.WindowScale;
            //         int yPos = 
            //             (i / Constants.DisplayHeight) * Constants.WindowScale;

            //         Console.WriteLine($"{xPos}, {yPos}");

            //         Raylib.DrawRectangle(
            //             xPos,
            //             yPos,
            //             Constants.WindowScale, 
            //             Constants.WindowScale, 
            //             Color.White
            //         );
            //     }
            // }

            if (updateTexture)
            {
                Raylib.UpdateTexture(texture, chip8.Display);
                Raylib.DrawTextureEx(
                    texture, 
                    new Vector2(0, 0), 
                    0.0f, 
                    Constants.WindowScale, 
                    Color.White);
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}