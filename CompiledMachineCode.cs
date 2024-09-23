namespace ConsoleApp7
{
    internal class CompiledMachineCode<T>(IntPtr memory, T codeDelegate) : IDisposable where T: Delegate
    {
        public T CodeDelegate => codeDelegate;
        public void Dispose()
        {
           Kernel32.VirtualFree(memory, 0, Kernel32.FreeType.Release);
        }
    }
}
