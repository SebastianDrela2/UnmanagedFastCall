namespace ConsoleApp7
{
    internal struct MachineCodeLine(byte[] bytes, string assemblyText, string description)
    {
        public readonly string AssemblyText = assemblyText;
        public readonly string Description = description;
        public readonly byte[] Bytes = bytes;

        public override string ToString()
        {
            var bytesText = $"[{string.Join(" ", Bytes.Select(@byte => $"{@byte:X2}"))}]";
            return $"{bytesText,-31} // {AssemblyText,-33}; {Description}";
        }
    }
}
