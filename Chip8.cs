namespace Chip8Sharp;

public class Chip8 
{
	// RAM
	private byte[] memory = new byte[4096]; 
	
	// Register V0 to VF
	private byte[] registers = new byte[16]; 
	
    // Stack to jump to subroutines.
    private ushort[] executionStack = new ushort[16]; 
	
	// Timer
	private byte delayTimer;

	// Beep timer
	private byte soundTimer; 
	
	// An index on the execution stack. 
	// Can be called stack pointer.
	private byte sp; 

	// Register simply called I in Chip8 documentation.
	private ushort index;

	// Program Counter
	private ushort pc; 

	private byte[] keypad = new byte[16];

	private byte randomByte;
	private ushort opcode; 

	private bool[] video = new bool[64 * 32];

	private readonly byte[] font = new byte[] {
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
	}; 

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
		if (!File.Exists(fileName) || 
			string.IsNullOrEmpty(fileName))
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
			Console.WriteLine($"Rom is larger than available" + 
				"memory. (CURRENT MEMORY SIZE: {memory.Length})"
			);
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

	// 00E0 - CLS
	// Clear display.
	public void Instruct_00E0() 
	{
		for (int i = 0; i < video.Length; i++)
		{
			video[i] = false;
		}
	}

	// 00EE - RET
	// Return from a subroutine.
	public void Instruct_00EE() 
	{
		sp--;
		pc = executionStack[sp];
	}

	// 1nnn - JP addr
	// Jump to location nnn.
	public void Instruct_1nnn() 
	{
		ushort address = (ushort) (opcode & 0x0FFF);

		pc = (ushort) address;
	}

	// 2nnn - CALL addr
	// Call subroutine at location nnn.
	public void Instruct_2nnn()
	{
		executionStack[sp] = pc;
		sp++;

		ushort address = (ushort) (opcode & 0x0FFF);
		pc = address;
	}

	// 3xkk - SE Vx, byte
	// Skip next instruction if Vx == kk.
	public void Instruct_3xkk()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte kk = (byte) (opcode & 0x00FF);

		if (registers[x] == kk)
		{
			pc += 2;
		}
	}

	// 4xkk - SNE Vx, byte
	// Skip next instruction if Vx != kk.
	public void Instruct_4xkk() 
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte kk = (byte) (opcode & 0x00FF);

		if (registers[x] != kk)
		{
			pc += 2;
		}
	}

	// 5xy0 - SE Vx, Vy
	// Skip next instruction if Vx = Vy;
	public void Instruct_5xy0()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte y = (byte) ((opcode & 0x00F0) >> 4);

		if (registers[x] == registers[y])
		{
			pc += 2;
		}
	}

	// 6xkk - LD Vx, byte
	// Set Vx = kk.
	public void Instruct_6xkk()
	{
		byte x = (byte) ((opcode & 0x0F00) & 8);
		byte kk = (byte) (opcode & 0x00FF);

		registers[x] = kk;
	}

	// 7xkk - ADD Vx, byte
	// Set Vx = Vx + kk.
	public void Instruct_7xkk()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte kk = (byte) (opcode & 0x00FF);

		registers[x] += kk;
	}

	// 8xy0 - LD Vx, Vy
	// Set Vx = Vy.
	public void Instruct_8xy0()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte y = (byte) ((opcode & 0x00F0) >> 4);

		registers[x] = registers[y];
	}

	// 8xy1 - OR Vx, Vy
	// Set Vx = Vx OR Vy
	public void Instruct_8xy1()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte y = (byte) ((opcode & 0x00F0) >> 4);

		registers[x] |= registers[y]; 
	}

	// 8xy2 - AND Vx, Vy
	// Set Vx = Vx AND Vy.
	public void Instruct_8xy2() 
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte y = (byte) ((opcode & 0x00F0) >> 4);

		registers[x] &= registers[y];
	}

	// 8xy3 - XOR Vx, Vy
	// Set Vx = Vx XOR Vy.
	public void Instruct_8xy3()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte y = (byte) ((opcode & 0x00F0) >> 4);

		registers[x] ^= registers[y];
	}

	// 8xy4 - ADD Vx, Vy
	// Set Vx = Vx + Vy, set VF = carry.
	public void Instruct_8xy4()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte y = (byte) ((opcode & 0x00F0) >> 4);

		var result = registers[x] + registers[y];

		if (result > byte.MaxValue)
		{
			registers[0xF] = 1;
		}
		else
		{
			registers[0xF] = 0;
		}

		registers[x] = (byte) (result & 0x00FF);
	}

	// 8xy5 - SUB Vx, Vy
	// Set Vx = Vx - Vy, set VF = NOT borrow.
	public void Instruct_8xy5()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte y = (byte) ((opcode & 0x00F0) >> 4);

		if (registers[x] > registers[y])
		{
			registers[0xF] = 1;
		}
		else
		{
			registers[0xF] = 0;
		}

		registers[x] -= registers[y];
	}


	// 8xy6 - SHR Vx {, Vy}
	// Set Vx = Vx SHR 1.
	public void Instruct_8xy6()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);

		registers[0xF] = (byte) (registers[x] & 0x0001);

		registers[x] >>= 1;
	}
}