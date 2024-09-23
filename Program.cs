using ConsoleApp7;

partial class Program
{
    internal delegate int MyUnmanagedDelegate();
    internal delegate int MyManagedDelegate(int a, int b);
    internal static unsafe void Main(string[] args)
    {
        /*
         * rsp is aligned to 16 bytes
         * call machine code function
         * (call) push ret adress 8 bytes, jump
         */
        var builder = new MachineCodeBuilder<MyUnmanagedDelegate>();
        builder.AddProlog();
        builder.SetRax<MyManagedDelegate>(MyMagicManagedMethod);
        builder.AddInstruction([0xB9, 0x05, 0x00, 0x00, 0x00], "mov  ecx, 0x05", "Move to ecx 5");
        builder.AddInstruction([0xBA, 0x0A, 0x00, 0x00, 0x00], "mov  edx, 0x0A", "Move to edx 10");
        builder.AddInstruction([0xFF, 0xD0],                   "call rax"      , "Call the function pointer in RAX");
        builder.AddInstruction([0x83, 0xC0, 0x64],             "add  eax, 0x64", "Add eax 100");
        builder.AddEpilog();

        using var compiledMachineCode = builder.Compile();
        var result = compiledMachineCode.CodeDelegate();

        Console.WriteLine($"Actual result of {nameof(MyMagicManagedMethod)} is {result}");
        Console.WriteLine();
        Console.WriteLine(builder.AssemblyText);
    }

    private static int MyMagicManagedMethod(int a, int b)
    {
        var sum = a + b;

        Console.WriteLine("Jumped to managed method!");
        Console.WriteLine();
        Console.WriteLine($"Added {a} and {b} result {sum}?");
        Console.WriteLine();
        return sum;
    }
}
