using ConsoleApp7;
using System.Runtime.InteropServices;

partial class Program
{
    internal delegate int MyUnmanagedDelegate();
    internal delegate int MyManagedDelegate(int a, int b);
    internal static unsafe void Main(string[] args)
    {             
        var mcWriter = new MachineCodeBuilder<MyUnmanagedDelegate>();        
        mcWriter.AddInstruction([0x48, 0x83, 0xEC, 0x28],       "sub rsp, 0x28", "Align stack (16-byte alignment)");       
        mcWriter.SetRax<MyManagedDelegate>(MyMagicManagedMethod);
        mcWriter.AddInstruction([0xB9, 0x05, 0x00, 0x00, 0x00], "mov ecx, 0x05" , "Move to ecx 5");
        mcWriter.AddInstruction([0xBA, 0x0A, 0x00, 0x00, 0x00], "mov edx, 0x0A", "Move to edx 10");
        mcWriter.AddInstruction([0xFF, 0xD0],                   "call rax"     , "Call the function pointer in RAX");
        mcWriter.AddInstruction([0x83, 0xC0, 0x64],             "add eax, 0x64", "Add eax 100");
        mcWriter.AddInstruction([0x48, 0x83, 0xC4, 0x28],       "add rsp, 0x28", "Restore stack");
        mcWriter.AddInstruction([0xC3],                         "ret"          , "Return");

        using var compiledMachineCode = mcWriter.Compile();
        var result = compiledMachineCode.CodeDelegate();

        Console.WriteLine($"Actual result of {nameof(MyMagicManagedMethod)} is {result}");
        Console.WriteLine(mcWriter.InstructionText);
    }

    private static int MyMagicManagedMethod(int a, int b)
    {
        var sum = a + b;

        Console.WriteLine("Jumped to managed method!");
        Console.WriteLine($"Added {a} and {b} result {sum}?");
        return sum;
    }
}
