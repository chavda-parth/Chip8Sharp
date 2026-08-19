namespace Chip8Sharp;

public class Chip8 
{
	// RAM
	private readonly byte[] memory = new byte[4096]; 
	
	// Register V0 to VF
	private readonly byte[] registers = new byte[16]; 
	
    // Stack to jump to subroutines.
    private readonly ushort[] executionStack = new ushort[16]; 
	
	// Timer
	private byte delayTimer;

	// Beep timer
	private byte soundTimer; 
	
	// An index on the execution stack. 
	// Can be called stack pointer.
	private byte sp; 

	// Register simply called I in Chip8 documentation.
	private ushort registerI;

	// Program Counter
	private ushort pc; 

	private readonly byte[] keypad = new byte[16];

	private ushort opcode; 

	private readonly bool[] display = 
		new bool[Constants.DisplayWidth * Constants.DisplayHeight];

	public ReadOnlySpan<bool> Display => display;

	private readonly byte[] font = new byte[] 
	{
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

	private Random? rng;

	public Chip8() 
	{
		// Load font into memory.
		for (int i = 0; i < font.Length; i++)
		{
			memory[Constants.FontStartAddress + i] = font[i];
		}

		pc = Constants.RomStartAddress;
	}

	public bool TryLoadRom(string? fileName)
	{
		// Check if rom file exists.
		if (!File.Exists(fileName) || 
			string.IsNullOrEmpty(fileName))
		{
			Console.WriteLine("Could not find rom.");
			return false;
		}

		// Read rom file as a byte array.
		var rom = File.ReadAllBytes(fileName);

		if (rom == null || rom.Length == 0)
		{
			Console.WriteLine("Corrupted rom.");
			return false;
		}

		if (rom.Length > memory.Length)
		{
			Console.WriteLine($"Rom is larger than available memory." + 
				"(CURRENT MEMORY SIZE: {memory.Length})");
			return false;
		}

		// Load the rom into memory.
		for (int i = 0; i < rom.Length; i++)
		{
			memory[Constants.RomStartAddress + i] = rom[i];
		}

		// Get ticks since unix epoch.
		var ticks = new DateTimeOffset(DateTime.Now).Ticks;

		// Get new Random instance with calculated ticks as seed.
		rng = new Random((int) ticks);

		return true;
	}

	private byte GetRandomByte()
	{
		Span<byte> buffer = stackalloc byte[1];
		rng?.NextBytes(buffer);

		return buffer[0];
	}

#region Chip-8 Instructions

	// 00E0 - CLS
	// Clear display.
	public void Instruct_00E0() 
	{
		for (int i = 0; i < display.Length; i++)
		{
			display[i] = false;
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
		byte x = (byte) ((opcode & 0x0F00) >> 8);
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

		registers[0xF] = (byte) (registers[x] & 0x1);

		registers[x] >>= 1;
	}

	// 8xy7 - SUBN Vx, Vy
	// Set Vx = Vy - Vx, set VF = NOT borrow.
	public void Instruct_8xy7()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte y = (byte) ((opcode & 0x00F0) >> 4);

		if (registers[y] > registers[x])
		{
			registers[0xF] = 1;
		}
		else
		{
			registers[0xF] = 0;
		}

		registers[x] = (byte) (registers[y] - registers[x]);
	}


	// 8xyE - SHL Vx {, Vy}
	// Set Vx = Vx SHL 1.
	public void Instruct_8xyE()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);

		registers[0xF] = (byte) ((registers[x] & 0x80) >> 7);

		registers[x] <<= 1;
	}
 
	// 9xy0 - SNE Vx, Vy
	// Skip next instruction if Vx != Vy.
	public void Instruct_9xy0()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte y = (byte) ((opcode & 0x00F0) >> 4);

		if (registers[x] != registers[y])
		{
			pc += 2;
		}
	}

	// Annn - LD I, addr
	// Set I = nnn
	public void Instruct_Annn()
	{
		ushort address = (ushort) (opcode & 0x0FFF);

		registerI = address;
	}

	// Bnnn - JP V0, addr
	// Jump to location nnn + V0.
	public void Instruct_Bnnn()
	{
		ushort address = (ushort) (opcode & 0x0FFF);

		pc = (ushort) (address + registers[0]);
	}

	// Cxkk - RND Vx, byte
	// Set Vx = random byte AND kk.
	public void Instruct_Cxkk()
	{
		byte randomByte = GetRandomByte();
		byte kk = (byte) (opcode & 0x00FF);

		byte x = (byte) ((opcode & 0x0F00) >> 8);

		registers[x] = (byte) (randomByte & kk);
	}

	// Dxyn - DRW Vx, Vy, nibble
	// Display n-byte sprite starting at memory location I at (Vx, Vy), 
	// set VF = collision
	public void Instruct_Dxyn()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
		byte y = (byte) ((opcode & 0x00F0) >> 4);
		byte n = (byte) (opcode & 0x000F);

		byte xPos = (byte) (registers[x] % Constants.DisplayWidth);
		byte yPos = (byte) (registers[y] % Constants.DisplayHeight);

		registers[0xF] = 0;

		for (int row = 0; row < n; row++)
		{
			byte pixels = memory[registerI + row];

			var calculatedY = yPos + row;

			if (calculatedY >= Constants.DisplayHeight) 
			{
				Console.WriteLine($"Clipping y. Value: {calculatedY}");
				continue;
			}

			for (int col = 0; col < 8; col++)
			{
				var calculatedX = xPos + col;

				if (calculatedX >= Constants.DisplayWidth) 
				{
					Console.WriteLine($"Clipping x. Value: {calculatedX}");
				}

				byte pixel = (byte) (pixels & (0x80 >> col));
				
				var screenPos = 
					calculatedY * Constants.DisplayWidth + calculatedX;

				if (pixel != 0)
				{
					if (display[screenPos])
					{
						registers[0xF] = 1;
					}

					display[screenPos] ^= true;
				}
			} 
		}
	}

	// Ex9E - SKP Vx
	// Skip next instruction if key with the value of Vx is pressed.
	public void Instruct_Ex9E()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);

		byte key = registers[x];

		if (keypad[key] != 0)
		{
			pc += 2;
		}
	}

	// ExA1 - SKNP Vx
	// Skip next instruction if key with the value of Vx is not pressed.
	public void Instruct_ExA1()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);

		byte key = registers[x];

		if (keypad[key] == 0)
		{
			pc += 2;
		}
	}

	// Fx07 - LD Vx, DT
	// Set Vx = delay timer value.
	public void Instruct_Fx07()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);

		registers[x] = delayTimer;
	}

	// Fx0A - LD Vx, K
	// Wait for a key press, store the value of the key in Vx.
	public void Instruct_Fx0A()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);

		for (int i = 0; i < keypad.Length; i++)
		{
			if (keypad[i] != 0)
			{
				registers[x] = (byte) i;
				return;
			}
		}

		pc -= 2;
	}

	// 	Fx15 - LD DT, Vx
	// Set delay timer = Vx.
	public void Instruct_Fx15()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);

		delayTimer = registers[x];
	}


	// Fx18 - LD ST, Vx
	// Set sound timer = Vx.
	public void Instruct_Fx18()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);

		soundTimer = registers[x];
	}

	// Fx1E - ADD I, Vx
	// Set I = I + Vx.
	public void Instruct_Fx1E()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);

		registerI += registers[x];
	}

	// Fx29 - LD F, Vx
	// Set I = location of sprite for digit Vx.
	public void Instruct_Fx29()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);
	
		byte digit = registers[x];

		registerI = (ushort) (Constants.FontStartAddress + (5 * digit));
	}


	// Fx33 - LD B, Vx
	// Store BCD representation of Vx in memory locations I, I+1, and I+2.
	public void Instruct_Fx33()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);

		byte target  = registers[x];

		memory[registerI] = (byte) (target / 100);
		target %= 100;

		memory[registerI + 1] = (byte) (target / 10);
		target %= 10;

		memory[registerI + 2] = target;
	}

	// Fx55 - LD [I], Vx
	// Store registers V0 through Vx in memory starting at location I.
	public void Instruct_Fx55()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);

		for (int i = 0; i <= x; i++)
		{
			memory[registerI + 1] = registers[i];
		}
	}

	// Fx65 - LD Vx, [I]
	// Read registers V0 through Vx from memory starting at location I.
	public void Instruct_Fx65()
	{
		byte x = (byte) ((opcode & 0x0F00) >> 8);

		for (int i = 0; i <= x; i++)
		{
			registers[i] = memory[registerI + i];
		}
	}

#endregion

	private void FetchInstruction()
	{
		byte highByte = memory[pc];
		byte lowByte = memory[pc + 1];

		pc += 2;

		opcode = (ushort) ((highByte << 8) | lowByte);
	}

	private void DecodeInstruction()
	{
		byte firstNibble = (byte) ((opcode & 0xF000) >> 12);
		byte n = (byte) (opcode & 0x000F);
		byte kk = (byte) (opcode & 0x00FF);

		switch (firstNibble)
		{
			case 0x0:
				switch (kk)
				{
					case 0xEE:
						Instruct_00EE();
						break;
					case 0xE0:
						Instruct_00E0();
						break;
				}
				break;
			
			case 0x1:
				Instruct_1nnn();
				break;
			
			case 0x2:
				Instruct_2nnn();
				break;

			case 0x3:
				Instruct_3xkk();
				break;
			
			case 0x4:
				Instruct_4xkk();
				break;

			case 0x5:
				if (n == 0)
				{
					Instruct_5xy0();
				}
				break;
			
			case 0x6:
				Instruct_6xkk();
				break;
			
			case 0x7:
				Instruct_7xkk();
				break;
			
			case 0x8:
				switch (n)
				{
					case 0x0:
						Instruct_8xy0();
						break;
					
					case 0x1:
						Instruct_8xy1();
						break;

					case 0x2:
						Instruct_8xy2();
						break;
					
					case 0x3:
						Instruct_8xy3();
						break;
					
					case 0x4:
						Instruct_8xy4();
						break;

					case 0x5:
						Instruct_8xy5();
						break;
					
					case 0x6:
						Instruct_8xy6();
						break;
					
					case 0x7:
						Instruct_8xy7();
						break;
					
					case 0xE:
						Instruct_8xyE();
						break;
				}
				break;
			
			case 0x9:
				if (n == 0)
				{
					Instruct_9xy0();
				}
				break;
			
			case 0xA:
				Instruct_Annn();
				break;

			case 0xB:
				Instruct_Bnnn();
				break;

			case 0xC:
				Instruct_Cxkk();
				break;

			case 0xD:
				Instruct_Dxyn();
				break;
			
			case 0xE:
				switch (kk)
				{
					case 0x9E:
						Instruct_Ex9E();
						break;

					case 0xA1:
						Instruct_ExA1();
						break;
				}
				break;
			
			case 0xF:
				switch (kk)
				{
					case 0x07:
						Instruct_Fx07();
						break;
					
					case 0x0A:
						Instruct_Fx0A();
						break;
					
					case 0x15:
						Instruct_Fx15();
						break;

					case 0x18:
						Instruct_Fx18();
						break;

					case 0x1E:
						Instruct_Fx1E();
						break;
					
					case 0x29:
						Instruct_Fx29();
						break;

					case 0x33:
						Instruct_Fx33();
						break;

					case 0x55:
						Instruct_Fx55();
						break;

					case 0x65:
						Instruct_Fx65();
						break;
				}
				break;
		}
	}

	public void Cycle()
	{
		FetchInstruction();
		DecodeInstruction();

		if (delayTimer > 0)
		{
			--delayTimer;
		}

		if (soundTimer > 0)
		{
			--soundTimer;
		}
	}
}