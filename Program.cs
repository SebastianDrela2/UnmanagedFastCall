using System.Runtime.InteropServices;

partial class Program
{
    internal static unsafe void Main(string[] args)
    {
        IntPtr memory = VirtualAlloc(
            IntPtr.Zero, 
            64, 
            AllocationType.Commit |
            AllocationType.Reserve, 
            PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE);

        if (memory == IntPtr.Zero)
        {
            Console.WriteLine("Memory allocation failed");
            return;
        }

        byte[] machineCode =
        [
            0x48, 0x83, 0xEC, 0x28,                         // sub rsp, 0x28       ; Align stack (16-byte alignment)
            0x48, 0xB8,                                     // mov rax, <address>  ; Move 64-bit address (function pointer) into RAX
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Placeholder for the function pointer (64-bit address)
            0xFF, 0xD0,                                     // call rax            ; Call the function pointer in RAX
            0x48, 0x83, 0xC4, 0x28,                         // add rsp, 0x28       ; Restore stack
            0xC3                                            // ret                 ; Return
        ];

        IntPtr functionPointer = Marshal.GetFunctionPointerForDelegate(MyManagedMethod);
        long funcAddr = functionPointer.ToInt64();

        Buffer.BlockCopy(BitConverter.GetBytes(funcAddr), 0, machineCode, 6, 8);
        Marshal.Copy(machineCode, 0, memory, machineCode.Length);

        var codeDelegate = Marshal.GetDelegateForFunctionPointer<Action>(memory);
        codeDelegate();

        VirtualFree(memory, 0, FreeType.Release);
    }

    private static void MyManagedMethod()
    {
        Console.WriteLine("Jumped to managed method!");
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, PAGE_PROTECTION_FLAGS flProtect);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool VirtualFree(IntPtr lpAddress, uint dwSize, FreeType dwFreeType);

    [Flags]
    public enum AllocationType : uint
    {
        Commit = 0x1000,
        Reserve = 0x2000,
    }

    [Flags]
    public enum PAGE_PROTECTION_FLAGS : uint
    {
        PAGE_EXECUTE_READWRITE = 0x40
    }

    [Flags]
    public enum FreeType : uint
    {
        Release = 0x8000,
    }
}
