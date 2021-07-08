using System;
using System.Collections.Generic;
using System.IO;

namespace PolyChip8
{
    public class CPU
    {
        public const byte DisplayWidth = 64;
        public const byte DisplayHeight = 32;
        private const ushort RamSize = 4096;
        private const byte StackSize = 16;
        private const byte NumVRegisters = 16;
        private const byte KeyboardSize = 16;
        private const ushort InstructionStartAddress = 0x200;
        
        private readonly byte[] _screen;
        private readonly Random _random;

        private bool _interrupt;
        
        /// <summary>
        /// RAM from location 0x000 -> 0xFFF
        /// </summary>
        public byte[] Ram { get; }

        /// <summary>
        /// Stores the keyboard pressed values
        /// </summary>
        public byte[] Keyboard;
        
        /// <summary>
        /// Holds the Stack memory.
        /// </summary>
        public ushort[] Stack { get; }
        
        /// <summary>
        /// This timer is intended to be used for timing
        /// the events of games. its value can be set and read;
        /// </summary>        
        public int DelayTimer;

        /// <summary>
        /// This timer is used for sound effects. When its value is non-zero
        /// a beeping sound is made.
        /// </summary>
        public int SoundTimer;

        /// <summary>
        /// The Chip-8 Has 16 8-bit V registers
        /// [0] -> [F]
        ///
        /// Register V[F] is special and is used by some
        /// operations as a carry, borrow or collision flag.
        /// </summary>
        public byte[] VRegisters { get; init; }

        // also identified as I
        public ushort AddressRegister { get; private set; }
        /// <summary>
        /// The program counter stores the address currently
        /// being operated on.
        /// </summary>
        public ushort ProgramCounter { get; private set; }
        /// <summary>
        /// Stores the address of the current area on the stack.
        /// </summary>
        public byte StackPointer { get; private set; }
        /// <summary>
        /// The current instruction address
        /// </summary>
        public ushort Instruction { get; private set; }
        /// <summary>
        /// Stores the size of the program in bytes.
        /// </summary>
        public long ProgramSize { get; private set; }

        /// <summary>
        /// The second nibble, used to look up one of the 16 X registers.
        /// </summary>
        public byte X { get; private set; }

        /// <summary>
        /// The third nibble, used to look up one of the 16 Y registers.
        /// </summary>
        public byte Y { get; private set; }

        /// <summary>
        /// The fourth nibble. A 4-bit number.
        /// </summary>
        public byte N { get; private set; }

        /// <summary>
        /// Second byte. 8-bit immediate.
        /// </summary>
        public byte Nn { get; private set; }

        /// <summary>
        /// 2nd, 3rd and 4th nibbles. 12-bit immediate.
        /// </summary>
        public ushort Nnn { get; private set; }

        /// <summary>
        /// Holds the disassembly of the currently loaded program.
        /// </summary>
        public List<string> Disassembly { get; private set; }

        /// <summary>
        /// Returns a monochrome representation of the Chip-8 Display.
        /// Colors in this array are represented as 0xAARRGGBB
        /// </summary>
        public uint[] Screen
        {
            get
            {
                var buffer = new uint[DisplayHeight * DisplayWidth];

                for (int i = 0; i < buffer.Length; i++)
                {
                    var px = _screen[i];
                    buffer[i] = (uint) ((0x00FFFFFF * px) | 0xFF000000);
                }

                return buffer;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CPU()
        {
            VRegisters = new byte[NumVRegisters];
            Ram = new byte[RamSize];
            Stack = new ushort[StackSize];
            Keyboard = new byte[KeyboardSize];
            _screen = new byte[DisplayWidth * DisplayHeight];
            _random = new Random();
            
            Reset();
            
            //Set Font Memory in Ram
            Ram[0x0050] = 0xF0; Ram[0x0051] = 0x90; Ram[0x0052] = 0x90; Ram[0x0053] = 0x90; Ram[0x0054] = 0xF0; //0
            Ram[0x0055] = 0x20; Ram[0x0056] = 0x60; Ram[0x0057] = 0x20; Ram[0x0058] = 0x20; Ram[0x0059] = 0x70; //1
            Ram[0x005A] = 0xF0; Ram[0x005B] = 0x10; Ram[0x005C] = 0xF0; Ram[0x005D] = 0x80; Ram[0x005E] = 0xF0; //2
            Ram[0x005F] = 0xF0; Ram[0x0060] = 0x10; Ram[0x0061] = 0xF0; Ram[0x0062] = 0x10; Ram[0x0063] = 0xF0; //3
            Ram[0x0064] = 0x90; Ram[0x0065] = 0x90; Ram[0x0066] = 0xF0; Ram[0x0067] = 0x10; Ram[0x0068] = 0x10; //4
            Ram[0x0069] = 0xF0; Ram[0x006A] = 0x80; Ram[0x006B] = 0xF0; Ram[0x006C] = 0x10; Ram[0x006D] = 0xF0; //5
            Ram[0x006E] = 0xF0; Ram[0x006F] = 0x80; Ram[0x0070] = 0xF0; Ram[0x0071] = 0x90; Ram[0x0072] = 0xF0; //6
            Ram[0x0073] = 0xF0; Ram[0x0074] = 0x10; Ram[0x0075] = 0x20; Ram[0x0076] = 0x40; Ram[0x0077] = 0x40; //7
            Ram[0x0078] = 0xF0; Ram[0x0079] = 0x90; Ram[0x007A] = 0xF0; Ram[0x007B] = 0x90; Ram[0x007C] = 0xF0; //8
            Ram[0x007D] = 0xF0; Ram[0x007E] = 0x90; Ram[0x007F] = 0xF0; Ram[0x0080] = 0x10; Ram[0x0081] = 0xF0; //9
            Ram[0x0082] = 0xF0; Ram[0x0083] = 0x90; Ram[0x0084] = 0xF0; Ram[0x0085] = 0x90; Ram[0x0086] = 0x90; //A
            Ram[0x0087] = 0xE0; Ram[0x0088] = 0x90; Ram[0x0089] = 0xE0; Ram[0x008A] = 0x90; Ram[0x008B] = 0xE0; //B
            Ram[0x008C] = 0xF0; Ram[0x008D] = 0x80; Ram[0x008E] = 0x80; Ram[0x008F] = 0x80; Ram[0x0090] = 0xF0; //C
            Ram[0x0091] = 0xE0; Ram[0x0092] = 0x90; Ram[0x0093] = 0x90; Ram[0x0094] = 0x90; Ram[0x0095] = 0xE0; //D
            Ram[0x0096] = 0xF0; Ram[0x0097] = 0x80; Ram[0x0098] = 0xF0; Ram[0x0099] = 0x80; Ram[0x009A] = 0xF0; //E
            Ram[0x009B] = 0xF0; Ram[0x009C] = 0x80; Ram[0x009D] = 0xF0; Ram[0x009E] = 0x80; Ram[0x009F] = 0x80; //F
        }

        /// <summary>
        /// Loads the requested Program ROM into RAM.
        /// </summary>
        /// <param name="file">The program file to load.</param>
        public void LoadROM(string file)
        {
            using (var fs = new FileStream(file, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    var buffer = new byte[fs.Length];

                    ProgramSize = fs.Length;

                    br.BaseStream.Seek(0, SeekOrigin.Begin);
                    br.Read(buffer, 0, (int) fs.Length);

                    for (int i = 0; i < fs.Length; i++)
                    {
                        Ram[0x200 + i] = buffer[i];
                    }
                }
            }
        }

        /// <summary>
        /// Clocks the CPU
        /// </summary>
        public void Clock()
        {
            Fetch();
            var op = GetOp(Instruction);
            op();
        }

        public void SetKeypress(byte key)
        {
            
        }

        /// <summary>
        /// This function Gets the operation pointer for a given opcode
        ///
        /// This is not an get and execute because the <see cref="Disassemble"/> method
        /// uses it in order to construct a disassembly of the program code without
        /// executing it.
        /// </summary>
        /// <param name="instruction">The opcode</param>
        /// <returns>a <see cref="Action"/> which represents the Operation to perform.</returns>
        public Action GetOp(ushort instruction)
        {
            return (instruction & 0xF000) switch
            {
                0x0000 => (instruction & 0x00FF) switch
                {
                    0xE0 => //Clear the screen.
                        CLS,
                    0xEE => //Return from sub.
                        RET,
                    _ => NoOp
                },
                0x1000 => //Jump to location NN
                    JP,
                0x2000 => //Call sub at NN
                    CALL,
                0x3000 => //Skip next instruction if Vx == nn.
                    SEVX,
                0x4000 => //Skip next instruction if Vx != nn.
                    SNE,
                0x5000 => //Skip next instruction if Vx == Vy
                    SEVXVY,
                0x6000 => //Set Vx = nn
                    LoadVxWithNN,
                0x7000 => // Set Vx = Vx + kk
                    AddNNtoVx,
                0x8000 => (instruction & 0x000F) switch
                {
                    0x00 => //Set Vx = Vy
                        LDVXVY,
                    0x01 => //Vx OR Vy
                        VxORVy,
                    0x02 => // Vx AND Vy
                        VxANDVy,
                    0x03 => // Vx XOR Vy
                        VxXORVy,
                    0x04 => // Vx = Vx + Vy, Set VF = carry.
                        VxADDVy,
                    0x05 => //Set Vx = Vx - Vy, Set VF = Not Borrow
                        VxSUBVy,
                    0x06 => //Vx >> 1
                        VxSHR,
                    0x07 => //Set Vx = Vy - Vx, Set VF = Not Borrow
                        VxSUBN,
                    0x0E => //Set Vx << 1
                        VxSHL,
                    _ => NoOp
                },
                0x9000 => // Skip next instruction if Vx != Vy
                    VxSNEVy,
                0xA000 => // Set I = nnn
                    LdI,
                0xB000 => // Jump to location nnn + V0
                    JpV0,
                0xC000 => // Set Vx = random & nn
                    VxRND,
                0xD000 => DRW,
                0xE000 => (instruction & 0x00FF) switch
                {
                    0x9E => // Skips next instruction if key with value of Vx is pressed.
                        VxSKP,
                    0xA1 => //Skips next instruction if key with the value of Vx is not pressed.
                        VxSKNP,
                    _ => NoOp
                },
                0xF000 => (instruction & 0x00FF) switch
                {
                    0x07 => //Set Vx = delay timer value.
                        LoadVxWithDt,
                    0x0A => // Wait for a key press, store the value of the key in Vx
                        Wait,
                    0x15 => // Set Delay Timer
                        SetDelay,
                    0x18 => // Set Sound Timer
                        SetSound,
                    0x1E => // I = I + Vx
                        IADDVX,
                    0x29 => // I = Sprite Digit Vx
                        LDFVX,
                    0x33 => // Store BCD Representation of Vx in memory location I, I+1 and I+2.
                        BCD,
                    0x55 => //Stores registers V0 - Vx in memory starting at location I
                        Dump,
                    0x65 => // Read registers V0 through Vx in memory starting at location I.
                        Load,
                    _ => NoOp
                },
                _ => NoOp
            };
        }

        /// <summary>
        /// Fetches the instruction to be executed.
        ///
        /// the combined 16 bit instruction is stored in
        /// <see cref="Instruction"/>
        ///
        /// <remarks>
        /// This function increments the
        /// program counter by 2
        /// </remarks>
        /// </summary>
        public void Fetch()
        {
            var hiByte = Ram[ProgramCounter];
            var loByte = Ram[ProgramCounter + 1];
            
            Instruction = (ushort) ((hiByte << 8) | loByte);

            Nnn = (ushort) (Instruction & 0x0FFF);
            N = (byte) (Instruction & 0x000F);
            X = (byte) ((Instruction & 0x0F00) >> 8);
            Y = (byte) ((Instruction & 0x00F0) >> 4);
            Nn = (byte) (Instruction & 0x00FF);
            
            if(!_interrupt)
                ProgramCounter += 2;
        }

        /// <summary>
        /// Calls machine code routine (RCA 1802 for COSMAC VIP)
        /// at address NNN. Not necessary for most ROMs.
        /// </summary>
        private void NoOp()
        {
            return;
        }

        /// <summary>
        /// Clears the screen.
        ///
        /// 0x00E0 - CLS
        /// </summary>
        private void CLS()
        {
            for (int x = 0; x < DisplayWidth; x++)
            {
                for (int y = 0; y < DisplayHeight; y++)
                {
                    _screen[x + y * DisplayWidth] = 0;
                    Screen[X + y * DisplayWidth] = 0;
                }
            }
        }

        /// <summary>
        /// Returns from a subroutine
        ///
        /// 0x000EE - RET
        /// </summary>
        private void RET()
        {
            ProgramCounter = Stack[StackPointer];
            StackPointer -= 1;
        }

        /// <summary>
        /// Jumps to address NNN.
        ///
        /// 0x1000 - JP
        /// </summary>
        private void JP()
        {
            ProgramCounter = Nnn;
        }

        /// <summary>
        /// Calls Subroutine at NNN.
        ///
        /// 0x2000 - CALL
        /// </summary>
        private void CALL()
        {
            //increment stack pointer.
            StackPointer++;
            //put current pc on the top of the stack
            Stack[StackPointer] = ProgramCounter;
            //set program counter to NNN.
            ProgramCounter = Nnn;
        }

        /// <summary>
        /// Skips the next instruction if VX equals NN.
        /// (Usually the next instruction is a jump to skip a code block);
        ///
        /// 0x3000 - SE
        /// </summary>
        private void SEVX()
        {
            if (VRegisters[X] == Nn)
                ProgramCounter += 2;
        }

        /// <summary>
        /// Skips next instruction if Vx != nn
        /// </summary>
        private void SNE()
        {
            if (VRegisters[X] != Nn)
                ProgramCounter += 2;
        }

        /// <summary>
        /// Skips the next instruction if Vx equals VY
        /// /// (Usually the next instruction is a jump to skip a code block);
        ///
        /// 0x4000 - SNE 
        /// </summary>
        private void SEVXVY()
        {
            if (VRegisters[X] == VRegisters[Y])
                ProgramCounter += 2;
        }


        /// <summary>
        /// Loads NN into VX
        /// </summary>
        private void LoadVxWithNN()
        {
            VRegisters[X] = Nn;
        }

        /// <summary>
        /// Adds NN to VX. (Carry flag is not changed)
        /// </summary>
        private void AddNNtoVx()
        {
            var vX = VRegisters[X];
            var result = vX += Nn;
            VRegisters[X] = result;
        }

        /// <summary>
        /// Sets Vx to the value of Vy
        /// </summary>
        private void LDVXVY()
        {
            VRegisters[X] = VRegisters[Y];
        }

        /// <summary>
        /// Sets Vx to Vx | Vy
        /// </summary>
        private void VxORVy()
        {
            var Vx = VRegisters[X];
            var Vy = VRegisters[Y];

            var result = Vx | Vy;

            VRegisters[X] = (byte) result;
        }

        /// <summary>
        /// Sets Vx to Vx and Vy
        /// </summary>
        private void VxANDVy()
        {
            var Vx = VRegisters[X];
            var Vy = VRegisters[Y];

            var result = Vx & Vy;

            VRegisters[X] = (byte) result;
        }


        /// <summary>
        /// Sets Vx to Vx ^ VY
        /// </summary>
        private void VxXORVy()
        {
            var Vx = VRegisters[X];
            var Vy = VRegisters[Y];

            var result = Vx ^ Vy;

            VRegisters[X] = (byte) result;
        }

        /// <summary>
        /// Adds Vy to Vx. VF is set to 1 when there's a carry
        /// and to 0 when there is not.
        /// </summary>
        private void VxADDVy()
        {
            var Vx = VRegisters[X];
            var Vy = VRegisters[Y];

            var result = Vx + Vy;

            if (result > 255)
                VRegisters[0xF] = 1;

            VRegisters[X] = (byte) result;
        }

        /// <summary>
        /// Vy is subtracted from Vx. VF is set to 0 when there's
        /// a borrow, and 1 when there is not.
        /// </summary>
        private void VxSUBVy()
        {
            var Vx = VRegisters[X];
            var Vy = VRegisters[Y];

            VRegisters[0xF] = 0;

            if (Vx > Vy)
                VRegisters[0xF] = 1;

            var result = Vx - Vy;

            VRegisters[X] = (byte) result;
        }

        /// <summary>
        /// Stores the least significant bit of Vx in Vf and then
        /// shift's Vx to the right by 1/
        /// </summary>
        private void VxSHR()
        {
            var vx = VRegisters[X];
            VRegisters[0xF] = (byte) (vx & 0x01);
            VRegisters[X] = (byte) (vx >> 1);
        }

        /// <summary>
        /// Sets Vx to Vy - Vx. Vf is set to 0 when there's a
        /// borrow, and 1 when there is not
        /// </summary>
        private void VxSUBN()
        {
            var vx = VRegisters[X];
            var vy = VRegisters[Y];

            VRegisters[0xF] = vx > vy ? (byte) 0 : (byte) 1;

            VRegisters[X] = (byte) (vy - vx);
        }

        /// <summary>
        /// Stores the most significant bit of Vx in VF and then shifts
        /// Vx to the left by 1.
        /// </summary>
        private void VxSHL()
        {
            var vx = VRegisters[X];
            
            VRegisters[0xF] = (byte) (vx >> 7);
            VRegisters[X] = (byte) (vx << 1);
        }

        /// <summary>
        /// Skips the next instruction if Vx does not equal Vy.
        /// (Usually the next instruction is a jump to skip a code block).
        /// </summary>
        private void VxSNEVy()
        {
            var vx = VRegisters[X];
            var vy = VRegisters[Y];

            if (vx != vy)
                ProgramCounter += 2;
        }

        /// <summary>
        /// sets I to the address NNN
        /// </summary>
        private void LdI()
        {
            AddressRegister = Nnn;
        }

        /// <summary>
        /// Jumps to the address NNN plus V0
        /// </summary>
        private void JpV0()
        {
            ProgramCounter = (ushort) (Nnn + VRegisters[0]);
        }

        /// <summary>
        /// Sets Vx to the result of a bitwise & operation on a
        /// random number (typically: 0 to 255) and NN
        /// </summary>
        private void VxRND()
        {
            byte number = (byte) _random.Next(0, 255);

            VRegisters[X] = (byte) (number & Nn);
        }

        /// <summary>
        /// Draws a sprite at coordinate (Vx, Vy) that has a width
        /// of 8 pixels and a height of N+1 pixels. Each row of 8 pixels
        /// is read as bit-coded starting from memory location I.
        ///
        /// I value does not change after the execution of this instruction.
        ///
        /// Vf is set to 1 if any screen pixels are flipped from set to unset when the
        /// sprite is drawn, and to - if that does not happen.
        /// </summary>
        private void DRW()
        {
            var vx = VRegisters[X];
            var vy = VRegisters[Y];
            VRegisters[0xF] = 0;
            
            var screenPx = _screen[vx + DisplayWidth * vy];
            
            for (int yLine = 0; yLine < N; yLine++)
            {
                byte sprite = Ram[AddressRegister + yLine];
                
                for (int xLine = 0; xLine < 8; xLine++)
                {
                    if ((sprite & (0x80 >> xLine)) != 0)
                    {
                        if (_screen[(xLine + vx) + (yLine + vy) * DisplayWidth] == 1)
                            VRegisters[0xF] = 1;

                        _screen[(xLine + vx) + (yLine + vy) * DisplayWidth] ^= 1;
                    }
                    
                }
            }
        }


        /// <summary>
        /// Skips the next instruction if the key stored in Vx
        /// is pressed.
        ///
        /// (Usually the next instruction is a jump to skip a code block).
        /// </summary>
        private void VxSKP()
        {
            var vx = VRegisters[X];

            if (Keyboard[vx] != 0)
                ProgramCounter += 2;
        }

        /// <summary>
        /// Skips the next instruction if the key stored in Vx is not pressed.
        ///
        /// /// (Usually the next instruction is a jump to skip a code block).
        /// </summary>
        private void VxSKNP()
        {
            var vx = VRegisters[X];

            if (Keyboard[vx] == 0)
                ProgramCounter += 2;
        }

        /// <summary>
        /// Sets Vx to the value of the delayTimer;
        /// </summary>
        private void LoadVxWithDt()
        {
            VRegisters[X] = (byte) DelayTimer;
        }

        /// <summary>
        /// A key press is awaited, and then stored in Vx.
        ///
        /// (Blocking Operation. All instructions halted until next key event).
        /// </summary>
        private void Wait()
        {
            for (byte i = 0; i < 0xF; i++)
            {
                if (Keyboard[i] != 0)
                {
                    VRegisters[X] = i;
                    _interrupt = false;
                }
                else
                    _interrupt = true;
            }
        }

        /// <summary>
        /// Sets the delay timer to Vx;
        /// </summary>
        private void SetDelay()
        {
            DelayTimer = VRegisters[X];
        }

        /// <summary>
        /// Sets the sound timer to Vx.
        /// </summary>
        private void SetSound()
        {
            SoundTimer = VRegisters[X];
        }

        /// <summary>
        /// Adds Vx to I. Vf is not affected
        /// </summary>
        private void IADDVX()
        {
            AddressRegister = (ushort) (VRegisters[X] + AddressRegister);
        }

        /// <summary>
        /// Sets I to the location of the sprite for the character in Vx.
        ///
        /// Characters 0-F (in hex) are represented by a 4x5 Font.
        /// </summary>
        private void LDFVX()
        {
            AddressRegister = VRegisters[X];
        }

        /// <summary>
        /// Stores the binary-coded decimal representation of Vx,
        /// with the most significant of three digits at the address in I,
        ///
        /// the middle digit at I+1
        ///
        /// and the least significant digit at I+2.
        /// </summary>
        private void BCD()
        {
            var vx = VRegisters[X];
            var h = vx / 100;
            var t = (vx / 10) % 10;
            var d = vx % 10;

            Ram[AddressRegister] = (byte) h;
            Ram[AddressRegister + 1] = (byte) t;
            Ram[AddressRegister + 2] = (byte) d;
        }

        /// <summary>
        /// Stores V0 to Vx in memory starting at address I.
        ///
        /// The offset from I is increased by 1 for each value written.
        /// but I itself is left unmodified.
        /// </summary>
        private void Dump()
        {
            for (int i = 0; i <= X; i++)
                Ram[AddressRegister + i] = VRegisters[i];

        }

        /// <summary>
        /// Fills V0 to Vx (including Vx) with values from memory starting at address I.
        /// The offset from I is Increased by 1 for each value written. but I itself is left unmodified.
        /// </summary>
        private void Load()
        {
            for (int i = 0; i <= X; i++) 
                VRegisters[i] = Ram[AddressRegister + i] ;
        }

        //Resets CPU to initial power on state.
        private void Reset()
        {
            //Clear the registers
            foreach (var register in VRegisters)
            {
                VRegisters[register] = 0;
            }

            //Clear Stack
            for (int i = 0; i < StackSize; i++)
            {
                Stack[i] = 0x0000;
            }
            

            DelayTimer = 0;
            SoundTimer = 0;
            AddressRegister = 0x0000;
            ProgramCounter = InstructionStartAddress;
            StackPointer = 0;
            Instruction = 0x0000;
            ProgramSize = 0;
            X = 0;
            Y = 0;
            N = 0;
            Nn = 0;
            Nnn = 0;
            
            //Clear the screen.
            CLS();
        }

        ///<summary>
        /// Disassembles the rom loaded into memory.
        /// </summary> 
        public void Disassemble()
        {
            var start = InstructionStartAddress;
            Disassembly = new List<string>();

            for (int pc = start; pc < Ram.Length; pc += 2)
            {
                var hiByte = Ram[pc];
                var loByte = Ram[pc + 1];

                var instruction = (ushort) ((hiByte << 8) | loByte);

                var op = GetOp(instruction);
                var opName = op.Method.Name;

                Disassembly.Add($"$0x{pc:x4}    {opName}    (0x{instruction:X4})");
            }
        }
    }
}