public static class Program
{
    public static void Main() 
    {
        Console.Write("Enter filename: ");

        string? fileName = Console.ReadLine();

        var chip8 = new Chip8();

        chip8.LoadRom(fileName);
    }
}