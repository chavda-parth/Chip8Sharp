using Raylib_cs;

public static class Program
{
    enum MovementDirection {
        LTR,
        RTL
    }

    public static void Main() 
    {
        Raylib.InitWindow(640, 320, "Testing Window");
        Raylib.SetTargetFPS(60);
        

        float x = 0;
        var moveDir = MovementDirection.LTR;
        int speed = 100;

        while (!Raylib.WindowShouldClose()) 
        {
            if (moveDir == MovementDirection.LTR && x >= 540)
            {
                moveDir = MovementDirection.RTL;
            }
            else if (moveDir == MovementDirection.RTL && x <= 0)
            {
                moveDir = MovementDirection.LTR;
            }

            x += (Raylib.GetFrameTime() * (moveDir == MovementDirection.LTR ? speed : -speed)); 

            // Console.WriteLine($"x: {x}");

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.White);            

            // Raylib.DrawText("Hello, World", 12, 12, 20, Color.Black);
            Raylib.DrawRectangle((int) x, 12, 100, 100, Color.Black);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}