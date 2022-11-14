namespace Nicolas2422.RpiNetapi.Models
{
    public readonly record struct Cpu(string Hardware, string Model, int Frequency, decimal Temperature, decimal Usage);
    public readonly record struct Memory(int Total, int Used, int Free, decimal Usage);
    public readonly record struct Storage(string Filesystem, string Type, int Size, int Used, int Avail, decimal Usage, string Mounted);
}
