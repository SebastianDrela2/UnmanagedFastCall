using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConsoleApp7;

internal partial class MachineCodeBuilder<TResultDelegate> where TResultDelegate : Delegate
{
    public string AssemblyText => GetAssemblyText();   
   
    public List<MachineCodeLine> Instructions = [];

    public void AddInstruction(byte[] byteLine, string assemblyText, string description)
    {
        Instructions.Add(new MachineCodeLine(byteLine, assemblyText, description));
    }

    public void SetRax<TCallDelegate>(TCallDelegate callDelegate, 
        [CallerArgumentExpression(nameof(callDelegate))]string? functionName = null) where TCallDelegate : Delegate
    {
        IntPtr functionPointer = Marshal.GetFunctionPointerForDelegate(callDelegate);
        long funcAddr = functionPointer.ToInt64();

        AddInstruction([0x48, 0xB8, .. BitConverter.GetBytes(funcAddr)], 
            $"mov rax, <{functionName}>", "Move 64-bit address (function pointer) into RAX");
    }

    public CompiledMachineCode<TResultDelegate> Compile()
    {
        var arrayBytes = GetArrayBytes(); 

        IntPtr memory = Kernel32.VirtualAlloc(
        IntPtr.Zero,
        (uint)arrayBytes.Length,
        Kernel32.AllocationType.Commit |
        Kernel32.AllocationType.Reserve,
        Kernel32.PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE);

        if (memory == IntPtr.Zero)
        {               
            throw new NotImplementedException("Memory allocation failed");
        }
        
        Marshal.Copy(arrayBytes, 0, memory, arrayBytes.Length);

        TResultDelegate codeDelegate = Marshal.GetDelegateForFunctionPointer<TResultDelegate>(memory);
        return new CompiledMachineCode<TResultDelegate>(memory, codeDelegate);
    }

    private byte[] GetArrayBytes()
    {
        return Instructions.SelectMany(x => x.Bytes).ToArray();
    }

    private string GetAssemblyText()
    {
       return string.Join("\n", Instructions.Select(x => x.ToString()));
    }  
}
