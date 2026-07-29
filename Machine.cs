public class Machine {
	private byte[] memory = new byte[4096]; // RAM
	private byte[] registers = new byte[16]; // Register V0 to VF
	private byte[] keypad = new byte[16];

	private ushort[] executionStack = new ushort[16]; // Stack to jump to subroutines.
	
	private byte delayTimer; // Timer
	private byte soundTimer; // Beep timer
	private byte stackPointer; // Pointer pointing to a location on the execution stack.

	private ushort index; // Register simply called I in docs.
	private ushort pc; // Program Counter

	public Machine() {} 
}