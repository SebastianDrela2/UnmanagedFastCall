using System.Runtime.InteropServices;

namespace ConsoleApp7
{
    public partial class Kernel32
    {
        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, PAGE_PROTECTION_FLAGS flProtect);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool VirtualFree(IntPtr lpAddress, uint dwSize, FreeType dwFreeType);

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
}
