using System;

namespace CpuTest
{
    class Program
    {
        static void Main(string[] args)
        {
            PolyChip8.CPU cpu = new PolyChip8.CPU();
            
            cpu.LoadROM("/home/kris/Projects/PolyChip8/ROMS/IBM Logo.ch8");

            while (true)
            {
                cpu.Clock();
            }
        }
    }
}