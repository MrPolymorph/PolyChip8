using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

namespace PolyChip8
{
    public class CPU
    {
        public byte DisplayWidth = 64;
        public byte DisplayHeight = 32;

        private List<Instruction> _lookupTable;
        private uint[] _screen;
        
        /// <summary>
        /// Random from location 0x000 -> 0xFFF
        /// </summary>
        private byte[] _ram = new byte[4096];
        private ushort[] _stack = new ushort[16];
        
        /// <summary>
        /// This timer is intended to be used for timing
        /// the events of games. its value can be set and read;
        /// </summary>
        private Stopwatch _delayTimer;
        
        /// <summary>
        /// This timer is used for sound effects. When its value is non-zero
        /// a beeping sound is made.
        /// </summary>
        private Stopwatch _soundTimer;

        
        public byte[] VRegisters { get; init; }

        // also identified as I
        public ushort AddressRegister { get; private set; }
        public ushort ProgramCounter { get; private set; }
        public byte StackPointer { get; private set; }
        public ushort Instruction { get; private set; }
        
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

        /// <summary>
        /// FLAGS
        /// </summary>
        private byte _vF;

        
        public uint[] Screen {get => _screen;}

        public CPU()
        {
            _screen = new uint[DisplayWidth*DisplayHeight];

            _lookupTable = new List<Instruction>()
            {
                new Instruction("NoOp", NoOp, 0),
                new Instruction("CLS", CLS, 0xE0),
                new Instruction("RET", RET, 0x00EE),
                new Instruction("JP", JP, 0x10),
                new Instruction("CALL", CALL, 0x20),
                new Instruction("SeVx", SE, 0x30),
                new Instruction("SNE", SNE, 0x40),
                new Instruction("SE Vx", SEV, 0x50),
                new Instruction("LD Vx", LoadVxWithNN, 0x60),
                new Instruction("LD Vx", AddNNtoVx, 0x70),
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
                new Instruction("LD I, addr", LdI, 0xA0),
                new Instruction("JP V0, addr", CALL, 0xB000),
                new Instruction("RND Vx", VxRND, 0xC000),
                new Instruction("DRW Vx, Vy", DRW, 0xD0),
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

            VRegisters = new byte[16];

            ProgramCounter = 0x0200;

            for (int i = 0; i < _screen.Length; i++)
            {
                _screen[i] = (uint) System.Drawing.Color.HotPink.ToArgb();
            }
        }

        public void LoadROM(string file)
        {
            using (var fs = new FileStream(file, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    var buffer = new byte[fs.Length];

                    br.BaseStream.Seek(0, SeekOrigin.Begin);
                    br.Read(buffer, 0, (int) fs.Length);

                    for (int i = 0; i < fs.Length; i++)
                    {
                        _ram[0x200 + i] = buffer[i];
                    }
                }
            }

        }

        public void Clock()
        {
            Fetch();
            
            CurrentInstruction.Operation();
        }

        private void Fetch()
        {
            var loByte = _ram[ProgramCounter];
            var hiByte = _ram[ProgramCounter + 1];
            
            Instruction = (ushort) ((hiByte << 8) | loByte);
            var strInst = Convert.ToString(Instruction, 16);
            CurrentInstruction = _lookupTable.FirstOrDefault(x => x.OpCode == (hiByte & 0xF0));

            ProgramCounter += 2;
            
            /* INST   X   Y     N
             *           (   NN   )
             *      (   NNN       )
             * 0000 0000 0000 0000
             * 
             */
            Nnn = (ushort) (Instruction & 0x0FFF);
            N = (byte) (Instruction & 0x000F);
            X = (byte) ((Instruction & 0x0F00) >> 8);
            Y = (byte) ((Instruction & 0x00F0) >> 4);
            Nn = (byte) (Instruction & 0x00FF);
            
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
            _screen = new uint[DisplayWidth * DisplayHeight];
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
            _stack[StackPointer] = ProgramCounter;
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
        private void SE()
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
        private void SEV()
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
                _vF = 1;

        }

        /// <summary>
        /// Vy is subtracted from Vx. VF is set to 0 when there's
        /// a borrow, and 1 when there is not.
        /// </summary>
        private void VxSUBVy()
        {
            var Vx = VRegisters[X];
            var Vy = VRegisters[Y];
            
            _vF = 0;

            if (Vx > Vy)
                _vF = 1;
            
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

            VRegisters[X] = (byte)(Vx << 1);

            _vF = (byte) (Vx & 0b00000001);
        }

        /// <summary>
        /// Sets Vx to Vy - Vx. Vf is set to 0 when there's a
        /// borrow, and 1 when there is not
        /// </summary>
        private void VxSUBN()
        {
            
        }

        /// <summary>
        /// Stores the most significant bit of Vx in VF and then shifts
        /// Vx to the left by 1.
        /// </summary>
        private void VxSHL()
        {
            
        }

        /// <summary>
        /// Skips the next instruction if Vx does not equal Vy.
        /// (Usually the next instruction is a jump to skip a code block).
        /// </summary>
        private void VxSNEVy()
        {
            
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
            
        }

        /// <summary>
        /// Sets Vx to the result of a bitwise & operation on a
        /// reandom number (typically: 0 to 255) and NN
        /// </summary>
        private void VxRND()
        {
            
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
            var x = VRegisters[X] & DisplayWidth;
            var y = VRegisters[Y] & DisplayHeight;
            _vF = 0;

            var screenPx = _screen[x + DisplayWidth * y];
            
            
            for (int i = 0; i < N; i++)
            {
                byte sprite = _ram[AddressRegister];

                var array = new BitArray(new byte[] {sprite});
                
                for(int px = 0; px < array.Length; px++)
                {
                    bool spritePx = array[px];

                    if (spritePx && screenPx == System.Drawing.Color.White.ToArgb())
                    {
                        _screen[x + DisplayWidth * y] = (uint) System.Drawing.Color.Black.ToArgb();
                        _vF = 1;
                    }
                    else if (spritePx && screenPx != System.Drawing.Color.White.ToArgb())
                    {
                        _screen[x + DisplayWidth * y] = (uint) System.Drawing.Color.White.ToArgb();
                    }

                    if (px >= DisplayWidth)
                        break;
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
            
        }

        /// <summary>
        /// Sets the sound timer to Vx.
        /// </summary>
        private void SetSound()
        {
            
        }

        /// <summary>
        /// Adds Vx to I. Vf is not affected
        /// </summary>
        private void IADDVX()
        {
            
        }

        /// <summary>
        /// Sets I to the location of the sprite for the character in Vx.
        ///
        /// Characters 0-F (in hex) are represented by a 4x5 Font.
        /// </summary>
        private void LDFVX()
        {
            
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
            
        }

        /// <summary>
        /// Stores V0 to Vx in memory starting at address I.
        ///
        /// The offset from I is increased by 1 for each value written.
        /// but I itself is left unmodified.
        /// </summary>
        private void Dump()
        {
            
        }

        /// <summary>
        /// Fills V0 to Vx (including Vx) with values from memory starting at address I.
        /// The offset from I is Increased by 1 for each value written. but I itself is left unmodified.
        /// </summary>
        private void Load()
        {
            
        }

        public string Dissasembly()
        {
            var start = 0x200;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            
            for (int pc = start; pc < _ram.Length; pc += 2)
            {
                var loByte = _ram[pc];
                var hiByte = _ram[pc + 1];
            
                var instruction = (ushort) ((hiByte << 8) | loByte);
                var ci = _lookupTable.FirstOrDefault(x => x.OpCode == (hiByte & 0xF0));

                builder.AppendLine($"${pc:X4} {ci}");
            }

            return builder.ToString();
        }
    }
}