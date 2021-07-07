using System;
using System.Collections.Generic;
using System.IO;

namespace PolyChip8
{
    public class CPU
    {
        public byte DisplayWidth = 64;
        public byte DisplayHeight = 32;

        private byte[] _screen;

        private Random _random;

        /// <summary>
        /// RAM from location 0x000 -> 0xFFF
        /// </summary>
        public byte[] Ram { get; }

        private ushort[] Stack { get; }

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

        private List<Instruction> _lookupTable;
        
        public byte[] VRegisters { get; init; }

        // also identified as I
        public ushort AddressRegister { get; private set; }
        public ushort ProgramCounter { get; private set; }
        public byte StackPointer { get; private set; }
        public ushort Instruction { get; private set; }
        public long ProgramSize { get; private set; }

        public Instruction CurrentInstruction { get; private set; }

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

        public List<string> Dissassembly { get; private set; }

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

        public CPU()
        {
            _screen = new byte[DisplayWidth * DisplayHeight];
            _random = new Random();

            VRegisters = new byte[16];
            Dissassembly = new List<string>();
            ProgramCounter = 0x0200;
            Instruction = 0x0200;
            Ram = new byte[4096];
            Stack = new ushort[16];
            CurrentInstruction = new Instruction("NoOp", NoOp, 0x00);

            for (int i = 0; i < _screen.Length; i++)
            {
                _screen[i] = 0;
            }
            
            _lookupTable = new List<Instruction>()
            {
                new Instruction("NoOp", NoOp, 0),
                new Instruction("CLS", CLS, 0xE0),
                new Instruction("RET", RET, 0x00EE),
                new Instruction("JP", JP, 0x1000),
                new Instruction("CALL", CALL, 0x2000),
                new Instruction("SeVx", SEVX, 0x3000),
                new Instruction("SNE", SNE, 0x4000),
                new Instruction("SE Vx", SEVXVY, 0x5000),
                new Instruction("LD Vx", LoadVxWithNN, 0x6000),
                new Instruction("LD Vx", AddNNtoVx, 0x7000),
                new Instruction("LD Vx", LDVXVY, 0x800F),
                new Instruction("LD Vx", VxORVy, 0x8001),
                new Instruction("ADN Vx, Vy", VxANDVy, 0x8002),
                new Instruction("XOR Vx, Vy", VxXORVy, 0x8003),
                new Instruction("ADD Vx, Vy", VxADDVy, 0x8004),
                new Instruction("SUB Vx, Vy", VxSUBVy, 0x8005),
                new Instruction("SHR Vx {, Vy}", VxSHR, 0x8006),
                new Instruction("SUBN Vx, Vy", VxSUBN, 0x8007),
                new Instruction("Vx SHL Vx {, Vy}", VxSHL, 0x800E),
                new Instruction("SBE Vx, Vy", VxSNEVy, 0x9000),
                new Instruction("LD I, addr", LdI, 0xA000),
                new Instruction("JP V0, addr", CALL, 0xB000),
                new Instruction("RND Vx", VxRND, 0xC000),
                new Instruction("DRW Vx, Vy", DRW, 0xD000),
                new Instruction("SKP Vx", VxSKP, 0xE09E),
                new Instruction("SKNP Vx", VxSKNP, 0xE0A1),
                new Instruction("LD Vx, DT", LoadVxWithDt, 0xF007),
                new Instruction("LD Vx, K", Wait, 0xF00A),
                new Instruction("LD DT, Vx", SetDelay, 0xF015),
                new Instruction("ADD I, Vx", SetSound, 0xF018),
                new Instruction("LD F, Vx", LDFVX, 0xF029),
                new Instruction("LD B, VX", BCD, 0xF033),
                new Instruction("LD [I]. Vx", Dump, 0xF055),
                new Instruction("LD Vx, [I]", Load, 0xF065),
            };
            

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

        public void Clock()
        {
            Fetch();
            var op = GetOp(Instruction);
            op();
        }

        public Action GetOp(ushort instruction)
        {
            switch (instruction & 0xF000)
            {
                case 0x0000:
                {
                    switch (instruction )
                    {
                        case 0xE0: //Clear the screen.
                            return CLS;
                        case 0x0E: //Return from sub.
                            return RET;
                    }
                    
                    break;
                }
                case (0x1000): //Jump to location NN
                    return JP;
                case (0x2000): //Call sub at NN
                    return CALL;
                case (0x3000): //Skip next instruction if Vx == nn.
                    return SEVX;
                case (0x4000): //Skip next instruction if Vx != nn.
                    return SNE;
                case (0x5000): //Skip next instruction if Vx == Vy
                    return SEVXVY;
                case (0x6000): //Set Vx = nn
                    return LoadVxWithNN;
                case (0x7000): // Set Vx = Vx + kk
                    return AddNNtoVx;
                case (0x8000):
                {
                    switch (instruction & 0x000F)
                    {
                        case 0x00: //Set Vx = Vy
                            return LDVXVY;
                        case 0x01: //Vx OR Vy
                            return VxORVy;
                        case 0x02: // Vx AND Vy
                            return VxANDVy;
                        case 0x03: // Vx XOR Vy
                            return VxXORVy;
                        case 0x04: // Vx = Vx + Vy, Set VF = carry.
                            return VxADDVy;
                        case 0x05: //Set Vx = Vx - Vy, Set VF = Not Borrow
                            return VxSUBVy;
                        case 0x06: //Vx >> 1
                            return VxSHR;
                        case 0x07: //Set Vx = Vy - Vx, Set VF = Not Borrow
                            return VxSUBN;
                        case 0x0E: //Set Vx << 1
                            return VxSHL;
                    }
                }
                    break;
                case (0x9000): // Skip next instruction if Vx != Vy
                    return VxSNEVy;
                case(0xA000): // Set I = nnn
                    return LdI;
                case(0xB000): // Jump to location nnn + V0
                    return JpV0;
                case (0xC000): // Set Vx = random & nn
                    return VxRND;
                case (0xD000):
                    return DRW;
                case (0xE000):
                {
                    switch (instruction & 0x00FF)
                    {
                        case (0x9E): // Skips next instruction if key with value of Vx is pressed.
                            return VxSKP;
                        case (0xA1): //Skips next instruction if key with the value of Vx is not pressed.
                            return VxSKNP;
                    }

                    break;
                }
                case (0xF000):
                {
                    switch (instruction & 0x00FF)
                    {
                        case 0x07: //Set Vx = delay timer value.
                            return LoadVxWithDt;
                        case 0x0A: // Wait for a key press, store the value of the key in Vx
                            return Wait;
                        case 0x15: // Set Delay Timer
                            return SetDelay;
                        case 0x18: // Set Sound Timer
                            return SetSound;
                        case 0x1E: // I = I + Vx
                            return IADDVX;
                        case 0x29: // I = Sprite Digit Vx
                            return LDFVX;
                        case 0x33: // Stire BCD Representation of Vx in memory location I, I+1 and I+2.
                            return BCD;
                        case 0x55: //Stores registers V0 - Vx in memory starting at location I
                            return Dump;
                        case 0x65: // Read registers V0 through Vx in memory starting at location I.
                            return Load;
                    }

                    break;
                }
            }

            return () => { };
        }

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
            ProgramCounter = StackPointer;
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
            //put current pc on the top of the stack
            Stack[StackPointer] = ProgramCounter;
            //increment stack pointer.
            StackPointer++;
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
            var Vx = VRegisters[X];
            var Vy = VRegisters[Y];

            VRegisters[X] = (byte) (Vx << 1);

            VRegisters[0xF] = (byte) (Vx & 0b00000001);
        }

        /// <summary>
        /// Sets Vx to Vy - Vx. Vf is set to 0 when there's a
        /// borrow, and 1 when there is not
        /// </summary>
        private void VxSUBN()
        {
            var vx = VRegisters[X];
            var vy = VRegisters[Y];

            
            VRegisters[0xF] = (byte) (vy > vx ? 1 : 0);
            
            VRegisters[X] = (byte) (vy - vx);
        }

        /// <summary>
        /// Stores the most significant bit of Vx in VF and then shifts
        /// Vx to the left by 1.
        /// </summary>
        private void VxSHL()
        {
            var vx = VRegisters[X];
            
            VRegisters[0xF] = (byte) (((vx & 0xF0) == 1 ? 1 : 0) << 1);
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
            
        }

        /// <summary>
        /// Skips the next instruction if the key stored in Vx is not pressed.
        ///
        /// /// (Usually the next instruction is a jump to skip a code block).
        /// </summary>
        private void VxSKNP()
        {
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
        /// (Blocking Opeartion. All instructions halted until next key event).
        /// </summary>
        private void Wait()
        {
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
            byte h = (byte) Math.Abs(vx / 100 % 10);
            byte t = (byte) Math.Abs(vx / 10 % 10);
            byte d = (byte) Math.Abs(vx / 1 % 10);

            Ram[AddressRegister] = h;
            Ram[AddressRegister + 1] = t;
            Ram[AddressRegister + 2] = d;
        }

        /// <summary>
        /// Stores V0 to Vx in memory starting at address I.
        ///
        /// The offset from I is increased by 1 for each value written.
        /// but I itself is left unmodified.
        /// </summary>
        private void Dump()
        {
            var index = AddressRegister;

            for (int i = 0; i < X; i++)
            {
                Ram[index] = VRegisters[i];
                index++;
            }
        }

        /// <summary>
        /// Fills V0 to Vx (including Vx) with values from memory starting at address I.
        /// The offset from I is Increased by 1 for each value written. but I itself is left unmodified.
        /// </summary>
        private void Load()
        {
            var index = AddressRegister;

            for (int i = 0; i < X; i++)
            {
                VRegisters[i] = Ram[index];
                index++;
            }
        }

        public void Dissassemble()
        {
            var start = 0x200;


            for (int pc = start; pc < Ram.Length; pc += 2)
            {
                var hiByte = Ram[pc];
                var loByte = Ram[pc + 1];

                var instruction = (ushort) ((hiByte << 8) | loByte);

                var op = GetOp(instruction);
                var opName = op.Method.Name;

                Dissassembly.Add($"$0x{pc:x4}    {opName}    (0x{instruction:X4})");
            }
        }
    }
}