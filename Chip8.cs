namespace Chip8Sharp;

public class Chip8 
{
	private byte[] memory = new byte[4096]; // RAM
	private byte[] registers = new byte[16]; // Register V0 to VF
	private byte[] keypad = new byte[16];

	private ushort[] executionStack = new ushort[16]; // Stack to jump to subroutines.
	
	private byte delayTimer; // Timer
	private byte soundTimer; // Beep timer
	private byte stackPointer; // Pointer pointing to a location on the execution stack.
	private byte randomByte;

	private ushort index; // Register simply called I in docs.
	private ushort pc; // Program Counter
	private ushort opcode; 

	private bool[] video = new bool[64 * 32];

	private readonly byte[] font = [
		0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
		0x20, 0x60, 0x20, 0x20, 0x70, // 1
		0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
		0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
		0x90, 0x90, 0xF0, 0x10, 0x10, // 4
		0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
		0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
		0xF0, 0x10, 0x20, 0x40, 0x40, // 7
		0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
		0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
		0xF0, 0x90, 0xF0, 0x90, 0x90, // A
		0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
		0xF0, 0x80, 0x80, 0x80, 0xF0, // C
		0xE0, 0x90, 0x90, 0x90, 0xE0, // D
		0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
		0xF0, 0x80, 0xF0, 0x80, 0x80  // F
	]; 

	private const ushort ROM_START_ADDRESS = 0x200;
	private const ushort FONT_START_ADDRESS = 0x50;

	public Chip8() 
	{
		// Load font into memory.
		for (int i = 0; i < font.Length; i++)
		{
			memory[FONT_START_ADDRESS + i] = font[i];
		}

		pc = ROM_START_ADDRESS;
	}

	public void LoadRom(string? fileName)
	{
		// Check if rom file exists.
		if (!File.Exists(fileName) || string.IsNullOrEmpty(fileName))
		{
			Console.WriteLine("Could not find rom.");
			return;
		}

		// Read rom file as a byte array.
		var rom = File.ReadAllBytes(fileName);

		if (rom == null || rom.Length == 0)
		{
			Console.WriteLine("Corrupted rom.");
			return;
		}

		if (rom.Length > memory.Length)
		{
			Console.WriteLine($"Rom is larger than available memory. (CURRENT MEMORY SIZE: {memory.Length})");
			return;
		}

		// Load the rom into memory.
		for (int i = 0; i < rom.Length; i++)
		{
			memory[ROM_START_ADDRESS + i] = rom[i];
		}

		// Get ticks since unix epoch.
		var ticks = new DateTimeOffset(DateTime.Now).Ticks;

		// Get new Random instance with calculated ticks as seed.
		var rng = new Random((int) ticks);

		// Assign random byte using seeded Random object.
		Span<byte> buffer = stackalloc byte[1];
		rng.NextBytes(buffer);
		randomByte = buffer[0];
	} 
}