using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConsoleApp7;

internal class MachineCodeBuilder<TResultDelegate> where TResultDelegate : Delegate
{
    public string AssemblyText => GetAssemblyText();   
   
    private List<MachineCodeLine> _instructions = [];

    public void AddInstruction(byte[] byteLine, string assemblyText, string description)
    {
        _instructions.Add(new MachineCodeLine(byteLine, assemblyText, description));
    }

    public void AddProlog()
    {
        AddInstruction([0x48, 0x83, 0xEC, 0x20], "sub  rsp, 0x20", "Allocate 32 bytes shadow space");
        AddInstruction([0x48, 0x83, 0xEC, 0x08], "sub  rsp, 0x08", "Align to next 16 bytes, previous call added 8 bytes");
    }
    
    public void AddEpilog()
    {
       AddInstruction([0x48, 0x83, 0xC4, 0x28], "add  rsp, 0x28", "Restore stack (shadow space, alignment)");
       AddInstruction([0xC3]                  , "ret"           , "Return");
    }

    public void SetRax<TCallDelegate>(TCallDelegate callDelegate, 
        [CallerArgumentExpression(nameof(callDelegate))]string? functionName = null) where TCallDelegate : Delegate
    {
        IntPtr functionPointer = Marshal.GetFunctionPointerForDelegate(callDelegate);
        long funcAddr = functionPointer.ToInt64();

        AddInstruction([0x48, 0xB8, ..BitConverter.GetBytes(funcAddr)], 
            $"mov  rax, <{functionName}>", "Move 64-bit address (function pointer) into RAX");
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
        return _instructions.SelectMany(x => x.Bytes).ToArray();
    }

    private string GetAssemblyText()
    {
       return string.Join("\n", _instructions.Select(x => x.ToString()));
    }  
}
