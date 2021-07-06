using System;

namespace PolyChip8
{
    public struct Instruction
    {
        public string Name { get; }
        public Action Operation { get; }
        public int OpCode { get; }
        
        public Instruction(string name, Action operation, int opCode)
        {
            Name = name;
            Operation = operation;
            OpCode = opCode;
        }

        public override string ToString()
        {
            return $"{Name} :         0x{OpCode:X4}";
        }
    }
}