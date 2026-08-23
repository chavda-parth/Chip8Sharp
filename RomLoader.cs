namespace Chip8Sharp;

public static class RomLoader
{
    public static bool TryGetRom(out byte[]? rom)
    {
        rom = null;

        if (!Directory.Exists(Constants.RomsDir))
        {
            Console.WriteLine("Roms directory not found."  + 
                "Please keep your roms in <projectDirectory>/roms");
            return false;
        }
        else
        {
            string[] fileNames = Directory.GetFiles(Constants.RomsDir, "*.ch8");

            if (fileNames.Length == 0)
            {
                Console.WriteLine("No roms found. Exiting program.");
                return false;
            }

            Console.WriteLine("Select rom and press Enter:");

            var (Left, Top) = Console.GetCursorPosition();
            int selectionIndex = 0;

            bool updateSelection = true;

            while (rom == null)
            {
                if (!updateSelection) continue;

                updateSelection = false;
                Console.SetCursorPosition(Left, Top);

                for (int i = 0; i < fileNames.Length; i++)
                {
                    if (i == selectionIndex)
                    {
                        Console.Write("> ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }

                    Console.WriteLine(fileNames[i]);
                }

                var key = Console.ReadKey().Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selectionIndex = 
                        SelectIndex(selectionIndex - 1, fileNames.Length - 1);
                    updateSelection = true;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectionIndex = 
                        SelectIndex(selectionIndex + 1, fileNames.Length - 1);
                    updateSelection = true;
                } 
                else if (key == ConsoleKey.Enter)
                {
                    rom = File.ReadAllBytes(fileNames[selectionIndex]);
                }
            }

            return true;
        }
    }

    private static int SelectIndex(int index, int lastIndex)
    {
        if (index > lastIndex)
        {
            return 0;
        }

        if (index < 0)
        {
            return lastIndex;
        }

        return index;
    }
}