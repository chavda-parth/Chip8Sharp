using Raylib_cs;

namespace Chip8Sharp;

public static class Program
{
    private static readonly Dictionary<KeyboardKey, ConsoleKey> inputMap = 
        new Dictionary<KeyboardKey, ConsoleKey>()
        {
            {KeyboardKey.One, ConsoleKey.NumPad1},
            {KeyboardKey.Two, ConsoleKey.NumPad2},
            {KeyboardKey.Three, ConsoleKey.NumPad3},
            {KeyboardKey.Four, ConsoleKey.NumPad4},
            {KeyboardKey.Q, ConsoleKey.Q},
            {KeyboardKey.W, ConsoleKey.W},
            {KeyboardKey.E, ConsoleKey.E},
            {KeyboardKey.R, ConsoleKey.R},
            {KeyboardKey.A, ConsoleKey.A},
            {KeyboardKey.S, ConsoleKey.S},
            {KeyboardKey.D, ConsoleKey.D},
            {KeyboardKey.F, ConsoleKey.F},
            {KeyboardKey.Z, ConsoleKey.Z},
            {KeyboardKey.X, ConsoleKey.X},
            {KeyboardKey.C, ConsoleKey.C},
            {KeyboardKey.V, ConsoleKey.V},
        };

    public static void Main()
    {
        var chip8 = new Chip8();

        if (RomLoader.TryGetRom(out var rom) && rom != null)
        {
            chip8.LoadRom(rom);

            if (chip8.RomLoaded)
            {
                Start(chip8);
            }
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

            var currentTime = DateTime.UtcNow;
            float deltaTime =
                (float)(currentTime - lastCycleTime).TotalMilliseconds;

            if (deltaTime > Constants.CycleDelay)
            {
                lastCycleTime = currentTime;

                DetectInput(chip8);

                chip8.Cycle();

                DrawGraphics(chip8.Display);
            }

            Raylib.DrawFPS(12, 12);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    private static void DrawGraphics(ReadOnlySpan<bool> display)
    {
        int windowScale = Constants.WindowScale;
        int displayWidth = Constants.DisplayWidth;

        for (int i = 0; i < display.Length; i++)
        {
            if (!display[i]) continue;

            int xPos = (i % displayWidth) * windowScale;
            int yPos = (i / displayWidth) * windowScale;

            Raylib.DrawRectangle(
                xPos, yPos, windowScale, windowScale, Color.White);
        }
    }

    private static void DetectInput(Chip8 chip8)
    {
        foreach(var key in inputMap)
        {
            if (Raylib.IsKeyDown(key.Key))
            {
                chip8.SetKeyPress(key.Value, true);
                continue;
            }

            if (Raylib.IsKeyReleased(key.Key))
            {
                chip8.SetKeyPress(key.Value, false);
            }
        }
    }
}
