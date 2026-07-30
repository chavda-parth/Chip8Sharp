public class Chip8 {
	private byte[] memory = new byte[4096]; // RAM
	private byte[] registers = new byte[16]; // Register V0 to VF
	private byte[] keypad = new byte[16];

	private ushort[] executionStack = new ushort[16]; // Stack to jump to subroutines.
	
	private byte delayTimer; // Timer
	private byte soundTimer; // Beep timer
	private byte stackPointer; // Pointer pointing to a location on the execution stack.

	private ushort index; // Register simply called I in docs.
	private ushort pc; // Program Counter
	private ushort opcode; 

	private bool[] video = new bool[64 * 32];

	private const ushort START_ADDRESS = 0x200;

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
			memory[START_ADDRESS + i] = rom[i];
		}
	} 
}