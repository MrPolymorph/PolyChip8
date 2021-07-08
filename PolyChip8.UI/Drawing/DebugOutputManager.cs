using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PolyChip8.UI.Drawing
{
    public class DebugOutputManager
    {
        private SpriteFont _systemFont;
        private CPU _cpu;
        
        public DebugOutputManager(SpriteFont font, CPU cpu)
        {
            _systemFont = font;
            _cpu = cpu;
        }
            
        public void DrawRegisters(SpriteBatch sb)
        {
            var baseX = 70;
            var baseY = 320;
            
            
            sb.DrawString(_systemFont, $"PC: $0x{_cpu.ProgramCounter:X4}", new Vector2(baseX + 64, baseY + 25), Color.White);
            sb.DrawString(_systemFont, $"Stack P: $0x{_cpu.StackPointer:X2}", new Vector2(baseX + 64, baseY + 45), Color.White);
            sb.DrawString(_systemFont, $"I: $0x{_cpu.AddressRegister:X4} ", new Vector2(baseX + 64, baseY + 65), Color.White);
            
            sb.DrawString(_systemFont, $"X: $0x{_cpu.X:X2} ", new Vector2(baseX + 170, baseY + 65), Color.White);
            sb.DrawString(_systemFont, $"Y: $0x{_cpu.Y:X2} ", new Vector2(baseX + 260, baseY + 65), Color.White);
            sb.DrawString(_systemFont, $"N: $0x{_cpu.N:X2} ", new Vector2(baseX + 350, baseY + 65), Color.White);
            sb.DrawString(_systemFont, $"NN: $0x{_cpu.Nn:X2} ", new Vector2(baseX + 450, baseY + 65), Color.White);
            sb.DrawString(_systemFont, $"NNN: $0x{_cpu.Nnn:X4} ", new Vector2(baseX + 550, baseY + 65), Color.White);

            var y = (baseY + 100);
            var vx = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var x = (baseX + 62) + (j * 80);
                    sb.DrawString(_systemFont, $"V{vx:X2}: ${_cpu.VRegisters[i]:X2} ", new Vector2(x, y), Color.White);
                    vx++;
                }

                y += 25;
            }
        }

        public void DrawFps(SpriteBatch sb, float fps)
        {
            sb.DrawString(_systemFont, $"FPS: {fps}", new Vector2(0, 0), Color.Green);
        }

        public void DrawDisassembly(SpriteBatch sb)
        {
            var y = 450;
            var x = 150;
            
            var programCounterHex = _cpu.ProgramCounter.ToString("X4");
            var instruction = _cpu.Disassembly.FirstOrDefault(x => x.Contains(programCounterHex, StringComparison.OrdinalIgnoreCase));

            if (instruction != null)
            {
                var instructionIndex = _cpu.Disassembly.IndexOf(instruction);

                if (instructionIndex < 26)
                {
                    var dissPrint = _cpu.Disassembly.Take(26);

                    foreach (var line in dissPrint)
                    {
                        var color = Color.White;

                        if (line == instruction)
                            color = Color.Cyan;
                        
                        y += 20;
                        PrintDisassemblyLine(line, x, y, color, sb);
                    }
                }
                else
                {
                    var halfLines = 26 / 2;

                    var dissBefore = _cpu.Disassembly.Skip(instructionIndex - halfLines).Take(halfLines);
                    var dissAfter = _cpu.Disassembly.Skip(instructionIndex + 1).Take(halfLines);

                    foreach (var line in dissBefore)
                    {
                        y += 20;
                        PrintDisassemblyLine(line, x, y, Color.White, sb);
                    }

                    y += 20;
                    
                    PrintDisassemblyLine(instruction, x, y, Color.Cyan, sb);
                    
                    foreach (var line in dissAfter)
                    {
                        y += 20;
                        PrintDisassemblyLine(line, x, y, Color.White, sb);
                    }
                }
            }
        }
        
        private void PrintDisassemblyLine(string line, int x, int y, Color color, SpriteBatch sb)
        {
            sb.DrawString(_systemFont, line, new Vector2(x , y), color);
        }
        
        
        public void DrawRam(SpriteBatch sb)
        {
            int y = 28;
            int count = 0;
            
            for (int row = 0; row < 128; row++)
            {
                if (count >= _cpu.ProgramSize)
                    break;
                
                for (int col = 0; col < 28; col++)
                {
                    int index = 0x0200 + row *  y + col;

                    var lineColor = Color.White;
                    
                    if(index == _cpu.ProgramCounter || index == _cpu.ProgramCounter+1)
                        lineColor = Color.Red;
                    
                    sb.DrawString(_systemFont, _cpu.Ram[index].ToString("X2"), new Vector2(850 + (col * 25) , 0 + (row * 10)), lineColor
                        , 0.0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

                    count++;
                }
                
            }
        }
    }

}